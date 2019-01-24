/*
	Ara 3d Array Operations Library
	Copyright 2018, Ara 3D
	Usage permitted under terms of MIT Licenese

	This is a header-only library of generic algorithms that operate on random-access indexable containers such as  
	std::vector, std::array, and std::valarray from the STL, and raw C arrays. 
    It also works on ara3d::array, ara3d::func_array, and so forth. 
    This enhances algorithms found in the 
	STL library with algorithms designed specifically for arrays (like slicing, selecting, and striding). 

	Many of the function in this header return lightweight lazily evaluated arrays that are inspired by .NET Linq,
	but written in a more idiomatic C++ manner. 
*/

#pragma once

#include "array.h"
#include "algorithm.h"

#include <functional>
#include <type_traits>

namespace ara3d
{	
	template<typename T>
	using func = std::function<T(size_t)>;

	template<typename T>
	using farray = func_array<func<T>>;
	
	// Creates a functional array from the given function
	template<typename F>
	decltype(auto) make_farray(size_t n, const F& f) {
		return farray<decltype(f(0))>([](size_t n) { return f(n); });
	}

	// Returns an array of numbers from 0 .. n-1, that is lazily computed 
	inline farray<size_t> range(size_t n) {
		return farray<size_t>([](size_t i) { return i; }, n);
	}

	// Returns a functional array that consists of the range of numbers from 0 up to but not including a.size() 
	template<typename A>
	farray<size_t> indexes(const A& a) {
		return range(size(a));
	}

	// Creates a functional array that consists of the same element repeated n times.
	template<typename T>
	farray<T> repeat(const T& x, size_t n) {
		return farray<T>([](size_t) { return x; }, n);
	}

	// Returns an array containing a single unit 
	template<typename T>
	farray<T> unit(const T& x) {
		return repeat(x, 1);
	}

	// Returns a functional array that is created by applying a function to each element in a source array 
	template<typename A, typename F>
	decltype(auto) select(const A& a, const F& f) {
		return make_farray(a.size(), [](size_t n) { return f(a[n]); });
	}

	// Returns a functional array that is created by applying a function to each index from the source array 
	template<typename A, typename F>
	decltype(auto) select_indices(const A& a, const F& f) {
		return make_farray(a.size(), f);
	}

	// Returns a functional array that is created by applying a function to each element and index in a source array 
	template<typename A, typename F>
	decltype(auto) select_with_index(const A& a, const F& f) {
		return make_farray(a.size(), [](size_t n) { return f(a[n], n); });
	}

	// Returns a lightweight array that represents a range of a source container 
	template<typename A>
	decltype(auto) slice(const A& a, size_t from, size_t to = size(a)) {
		return const_array_slice<A>(begin(a) + from, to - from);
	}

	// Returns a lightweight array that represents a range of a source container 
	template<typename A>
	decltype(auto) slice(A& a, size_t from, size_t to = size(a)) {
		return array_slice<A>(begin(a) + from, to - from);
	}

	// Returns a lightweight immutable array that represents a range of a source container, and that jumps over elements  
	template<typename A>
	decltype(auto) slice(const A& a, size_t from, size_t to, size_t stride) {
		return const_array_stride<A>(begin(a) + from, (to - from) / stride, stride);
	}

	// Returns a lightweight immutable array that that jumps over elements  
	template<typename A>
	decltype(auto) stride(const A& a, size_t from, size_t stride) {
		return const_array_stride<A>(begin(a) + from, (size(a) - from) / stride, stride);
	}

	// Returns a lightweight immutable array that that jumps over elements  
	template<typename A>
	decltype(auto) stride(const A& a, size_t stride) {
		return const_array_stride<A>(begin(a), size(a) / stride, stride);
	}

	// Returns elements of the array, chosen by a list of indexes 
	template<typename ArrayT, typename IndexesT>
	decltype(auto) select_by_index(const ArrayT& vals, const IndexesT& idxs) {
		return select(idxs, [](size_t n) { return vals[n]; });
	}
	
	// Combines two arrays, creating a new functional array 
	template<typename ArrayT1, typename ArrayT2, typename F>
	decltype(auto) zip(const ArrayT1& xs, const ArrayT2& ys, const F& f) {
		return select_index(xs, [](size_t n) { return f(xs[n], ys[n]); });
	}

	// Combines two arrays, creating a new functional array 
	template<typename ArrayT1, typename ArrayT2, typename F>
	decltype(auto) zip_with_index(const ArrayT1& xs, const ArrayT2& ys, const F& f) {
		return select(xs, [](size_t n) { return f(xs[n], ys[n], n); });
	}

	// Returns the first element of the sequence
	template<typename A>
	decltype(auto) first(const A& xs) {
		return xs[0];
	}

	// Returns the last element in the sequence
	template<typename A>
	decltype(auto) last(const A& xs) {
		return xs[size(xs)-1];
	}

	// Converts an array into a vector by copying values.
	template<typename A>
	decltype(auto) to_vector(const A& xs) {
		return std::vector<typename A::value_type>(begin(xs), end(xs));
	}

	// Returns the index of the iterator. 
	template<typename C, typename IterT>
	size_t index(const C& c, const IterT& iter) {
		return (size_t)(iter - begin(c));
	}

	// Finds the first element satisfying specific criteria
	template<typename C, typename V>
	size_t find_index(const C& c, const V& v) {
		return index(c, find(c, v));
	}

	// Finds the first element satisfying specific criteria
	template<typename C, typename F>
	size_t find_index_if(const C& c, const F& f) {
		return index(c, find_if(c, f));
	}

	// Finds the first element satisfying specific criteria
	template<typename C, typename F>
	size_t find_index_if_not(const C& c, const F& f) {
		return index(c, find_if_not(c, f));
	}

    // Creates an array view given a collection 
    template<typename C>
    array_view<T> make_view(C& c) {
        return array_view<T>(c.begin(), c.size());
    }

    // Creates an array view given a pointer and size.
	template<typename T>
	array_view<T> make_view(T* begin, size_t size) {
		return array_view<T>(begin, size);
	}

    // Creates a readonly array view given a pointer and a size 
	template<typename T>
	const_array_view<T> make_const_view(const T* begin, size_t size) {
		return const_array_view<T>(begin, size);
	}
}
