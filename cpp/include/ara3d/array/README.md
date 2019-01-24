# array.h

A C++11 header-only library of array containers, views, and iterators that provide a standard interface to different layouts of data in memory, as well as to computed data. 

This is a single header file with no other dependencies (including STL) which means it is portable, fast to compile, and easy to include in different projects. 

Unlike [`std::array`](https://en.cppreference.com/w/cpp/container/array) the size of `ara3d::array` is specified in the constructor. It is rare in practice that array sizes are known at compile time. The `ara3d::array_view` is similar to [`stl::span`](https://en.cppreference.com/w/cpp/container/span) but permits writing of data elements. If read-only semantics are desired then the `ara3d::const_array_view` structure can be used.

## Design Rationale 

This library was motivated by a need to work with many different geometry and math libraries in an efficient way, while 
eliminating redudnancy of having to write different algorithms. It is also motivated by the fact that an enormous amount of code is written using `std::vector` when a proper array class would be significantly more efficient.

The design goals for this library:

1. Provide an efficient abstraction that is similar to `std::vector` for usage when data is already
allocated in memory and there is no need to support dynamic growing or shrinking of the array.

2. provide a compatible abstraction for functionally computed arrays that had O(1) memory consumption patterns (e.g. continous monotonically increasing values, repeated values, random sequences, transforms, etc.)

3. provide compatibility in terms of style and API with STL algorithms and modern C++ programming techniques. 

4. strive towards compliance with STL library, but not at cost of complexity and no need to support old versions of C++. Compare for example with https://github.com/martinmoene/span-lite/blob/master/include/nonstd/span.hpp

5. minimize dependencies - enable programmers to get just the code they need without         
               
Each array data structure provide the same interface: `begin()`/`end()` for using range-based for loops, a `size()` function, and an indexing operator. The iterators used are random-access forward only iterators. The data structures are compliant to a substantial  subset of the standard. Full standards compliance is possible, but would make the code a lot harder to read, validate, and longer to  compile. Only the array data structure allocates and deallocates memory for storage of elements.  

## Overview

The primary data structures are: 

* `array` - an array container of contiguous data in memory with ownership semantics (derived from array_view)
* `array_view` - a view into contiguous memory without ownership semantics
* `const_array_view` - a readonly view of contiguous memory without ownership semantics
* `array_slice` - a wrapper that provides access to a range of values in an existing array view   
* `const_array_stride` - a readonly wrapper around an array that jumps over N elements at a time 
* `array_mem_stride` - an array of values in memory that are a fixed number of bytes apart	
* `const_array_mem_stride` - a read only array of values in memory that are a fixed number of bytes apart
* `func_array` - an array that generates values on demand using a function 

 
All data structures implement the following interface:

```
iterator begin() const { return _iter; }
size_type size() const { return _size; }
iterator end() const { return begin() + size(); }
const value_type& operator[](size_t n) const { return begin()[n]; }
bool empty() const { return size() == 0; }
```

Additionally the non-readonly data structures implement the interface:

```
value_type& operator[](size_t n) { return begin()[n]; }
```
