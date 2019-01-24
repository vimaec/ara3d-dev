/*
    BFAST Binary Format for Array Streaming and Transmission
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License
*/
#pragma once

#define BFAST_VERSION { 0, 9, 0, "2018.12.24" }

#include <vector>
#include <assert.h>
#include <algorithm>
#include <iostream>
#include <iterator>
#include <fstream>

namespace bfast
{
    using namespace std;

    // Convenient typedefs (easier to read and type)
    typedef uint8_t byte;
    typedef uint64_t ulong;

    // Magic numbers for identifying a BFAST format
    const unsigned int MAGIC = 0xBFA5;
    const unsigned int SWAPPED_MAGIC = 0x5AFB;

    // The size of the header
    static const int header_size = 32;

    // The size of array offsets 
    static const int array_offset_size = 16;

    // This is the size of the header + padding to bring to alignment 
    static const int array_offsets_start = 64;

    // This is sufficient alignment to fit objects natively into 512-bit (64 byte) registers 
    static const int alignment = 64;

    // Returns true if the given value is aligned. 
    static bool is_aligned(size_t n) { return n % alignment == 0; }

    // Returns an aligned version of the given value to bring it to alignment 
    static size_t aligned_value(size_t n) {
        if (is_aligned(n)) return n;
        auto r = n + alignment - (n % alignment);
        assert(is_aligned(r));
        return r;
    }

    // The array offset indicates where in the raw byte array (offset from beginning of BFAST byte stream) that a particular array's data can be found. 
    struct ArrayOffset {
        ulong _begin;
        ulong _end;
    };

    // A data structure at the top of the file. This is followed by 32 bytes of padding, then an array of n array_offsets (where n is equal to num_arrays)
    struct alignas(8) Header {
        ulong magic;		// Either 0xBFA5 (same-endian) of 0x5AFB (different-endian)
        ulong data_start;	// >= desc_end and modulo 64 == 0 and <= file_size
        ulong data_end;		// >= data_start and <= file_size
        ulong num_arrays;	// number of array_headers
    };

    // A helper struct for representing a range of bytes 
    struct ByteRange {
        const byte* _begin;
        const byte* _end;
        const byte* begin() const { return _begin; }
        const byte* end() const { return _end; }
        size_t size() const { return end() - begin(); }
    };

    // Stores ranges of byte pointers to arrays and copies a BFAST into memory  
    struct Bfast
    {
        // When local copies of data are required 
        vector<vector<byte>> buffers;

        // Data is passed to a BfastBuilder as byte ranges
        vector<ByteRange> ranges;

        // Computes where the data offsets are relative to the beginning of the BFAST byte stream.
        vector<ArrayOffset> compute_offsets() {
            size_t n = compute_data_start();
            vector<ArrayOffset> r;
            for (auto range : ranges) {
                assert(is_aligned(n));
                ArrayOffset offset = { n, n + range.size() };
                r.push_back(offset);
                n += range.size();
                n = aligned_value(n);
            }
            return r;
        }

        // Computes where the first array data starts 
        size_t compute_data_start() {
            size_t r = 0;
            r += header_size;
            r = aligned_value(r);
            r += array_offset_size * ranges.size();
            r = aligned_value(r);
            return r;
        }

        // Computes how many bytes are needed to store the current BFAST blob
        size_t compute_needed_size() {
            auto tmp = compute_offsets();
            if (tmp.size() == 0)
                return compute_data_start();
            return tmp.back()._end;
        }

        // Copies the data structure to the bytes stream and update the current index
        template<typename T, typename OutIter_T>
        OutIter_T copy_to(T& x, OutIter_T out, int& current) {
            auto begin = (char*)&x;
            auto end = begin + sizeof(T);
            current += sizeof(T);
            return std::copy(begin, end, out);
        }

        // Adds zero bytes to the bytes stream for null padding 
        template<typename OutIter_T>
        OutIter_T output_padding(OutIter_T out, int& current) {
            while (!is_aligned(current)) {
                *out++ = (char)0;
                current++;
            }
            return out;
        }

        // Copies the BFAST data structure to the byte stream
        template<typename OutIter_T>
        OutIter_T copy_to_iterator(OutIter_T out) {
            // Initialize and get the data offsets 
            auto offsets = compute_offsets();
            assert(offsets.size() == ranges.size());
            auto n = offsets.size();
            auto current = 0;

            // Fill out the header
            Header h;
            h.magic = MAGIC;
            h.num_arrays = n;
            h.data_start = n == 0 ? 0 : offsets.front()._begin;
            h.data_end = n == 0 ? 0 : offsets.back()._end;

            // Copy the header and add padding 
            out = copy_to(h, out, current);
            out = output_padding(out, current);
            assert(is_aligned(current));

            // Early escape if there are no offsets 
            if (n == 0)
                return out;

            // Copy the array offsets and add padding 
            for (auto off : offsets)
                out = copy_to(off, out, current);
            out = output_padding(out, current);
            assert(is_aligned(current));
            assert(current = compute_data_start());

            // Copy the arrays 
            for (auto i = 0; i < ranges.size(); ++i) {
                auto range = ranges[i];
                auto offset = offsets[i];
                assert(current == offset._begin);
                out = copy(range.begin(), range.end(), out);
                current += range.size();
                assert(current == offset._end);
                output_padding(out, current);
            }
            return out;
        }

        // Outputs a G3D object to a strean
        ostream& copy_to_stream(ostream& out = cout) {
            copy_to_iterator(ostream_iterator<byte>(out));
            return out;
        }

        // Outputs a G3D object to a file
        ostream& copy_to_file(string path) {
            return copy_to_stream(ofstream(path, ofstream::out));
        }

        // Copies the G3D object into the vector, resizing it appropriately.
        vector<byte>& copy_to_bytes(vector<byte>& bytes) {
            bytes.resize(compute_needed_size());
            copy_to_iterator(bytes.data());
            return bytes;
        }

        // Copies a G3D object into a vector byte set 
        vector<byte> copy_to_bytes() {
            vector<byte> bytes;
            copy_to_bytes(bytes);
            return bytes;
        }

        // Adds a new range of bytes to G3D object. The range is not copied, only the pointers.
        void add_array(const byte* begin, const byte* end) {
            ranges.push_back({ begin, end });
        }

        // Adds a new range of bytes to G3D object. The range is not copied, only the pointers.
        void add_array(const void* begin, const void* end) {
            add_array((const byte*)begin, (const byte*)end);
        }

        // Adds a string as an array of bytes to a G3D object. The string is not copied, only the pointers.
        void add_string(string& s) {
            add_array(s.c_str(), s.c_str() + s.size());
        }

        // Converts a single object as an array of bytes and puts it in the G3D container. 
        template<typename T>
        void add_object_as_array(T* object) {
            add_array(object, object + 1);
        }

        // Stores a copy of the buffer and pushes the bytes into the G3D container.
        void copy_buffer(const vector<byte>& data) {
            buffers.push_back(data);
            auto back = buffers.back();
            add_array(back.data, back.data + back.size());
        }

        // Moves a buffer into local storate, and passes the range to the G3D container. 
        void move_buffer(vector<byte>&& data) {
            auto back = buffers.back();
            add_array(back.data, back.data + back.size());
        }
    };
}