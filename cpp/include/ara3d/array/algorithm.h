#pragma once

/*
	This is a C++14 wrapper around a number of STL algorithms that operate directly on an STL random access container, valarrays, 
	Boost range, or even a C style array. It also works of course on the various Ara 3D array classes.
	
	The underlying container is only required to supply an implementation of standalone 
	begin/end functions or begin/end member functions. 	
	
	The goal is to facilitate using the STL algorithms library by reducing the amount of boilerplate 
	programmers have to write, thus reducing chances of mistakes and making code more readable.  

	This library only has a dependency on the STL and does not introduce any new algorithms. All functions
	have trivial one line implementations. It is intended to be easy to 
	read and validate. That said, this library should work fine with Boost Range v2 or Eric Neiblers 
	Range v3 library.  

	See: 
	https://en.cppreference.com/w/cpp/algorithm
	https://www.reddit.com/r/cpp/comments/8he32w/wrapper_for_stl_algorithm_library/
	https://www.boost.org/doc/libs/develop/libs/range/doc/html/index.html
	https://www.boost.org/doc/libs/1_68_0/libs/algorithm/doc/html/index.html
	https://stackoverflow.com/questions/23033584/create-a-wrapper-for-std-algorithm-functions-to-take-container-instead-of-iterat
	https://softwareengineering.stackexchange.com/questions/231336/why-do-all-algorithm-functions-take-only-ranges-not-containers
	https://softwareengineering.stackexchange.com/questions/235403/wrapper-around-c-stl
	https://github.com/ericniebler/range-v3
*/

#include <algorithm>
#include <numeric>

namespace ara3d
{
	using std::begin;
	using std::end;

	//==================================================
	// Non-modifying sequence operations

	/// Checks if a predicate is true for all of the elements in a range 
	template<typename C, typename F>
	bool all_of(const C& c, const F& f) {
		return std::all_of(begin(c), end(c), f);
	}

	/// Checks if a predicate is true for any of the elements in a range 
	template<typename C, typename F>
	bool any_of(const C& c, const F& f) {
		return std::any_of(begin(c), end(c), f);
	}

	/// Checks if a predicate is true for none of the elements in a range 
	template<typename C, typename F>
	bool none_of(const C& c, const F& f) {
		return std::none_of(begin(c), end(c), f);
	}

	/// Applies a function to a range of elements
	template<typename C, typename F>
	F for_each(C& c, const F& f) {
		return std::for_each(begin(c), end(c), f);
	}

	/// Applies a function to a range of elements
	template<typename C, typename F>
	F for_each(const C& c, const F& f) {
		return std::for_each(begin(c), end(c), f);
	}

	/// Returns the number of elements satisfying specific criteria 
	template<typename C, typename V>
	size_t count(const C& c, const V& v) {
		return (size_t)std::count(begin(c), end(c), v);
	}

	/// Returns the number of elements satisfying specific criteria 
	template<typename C, typename F>
	decltype(auto) count_if(const C& c, const F& f) {
		return std::count_if(begin(c), end(c), f);
	}

	/// Finds the first position where two ranges differ
	template<typename C1, typename C2, typename F>
	decltype(auto) mismatch(const C1& c1, const C2& c2, const F& f) {
		return std::mismatch(begin(c1), end(c1), begin(c2), end(c2), f);
	}

	/// Finds the first element satisfying specific criteria
	template<typename C, typename V>
	decltype(auto) find(const C& c, const V& v) {
		return std::find(begin(c), end(c), v);
	}

	/// Finds the first element satisfying specific criteria
	template<typename C, typename F>
	decltype(auto) find_if(const C& c, const F& f) {
		return std::find_if(begin(c), end(c), f);
	}

	/// Finds the first element satisfying specific criteria
	template<typename C, typename F>
	decltype(auto) find_if_not(const C& c, const F& f) {
		return std::find_if_not(begin(c), end(c), f);
	}

	/// Finds the last sequence of elements in a certain range 
	template<typename C1, typename C2>
	decltype(auto) find_end(const C1& c1, const C2& c2) {
		return std::find_end(begin(c1), end(c1), begin(c2), end(c2));
	}

	/// Finds the last sequence of elements in a certain range 
	template<typename C1, typename C2, typename F>
	decltype(auto) find_end(const C1& c1, const C2& c2, const F& f) {
		return std::find_end(begin(c1), end(c1), begin(c2), end(c2), f);
	}

	/// Searches for any one of a set of elements
	template<typename C1, typename C2>
	decltype(auto) find_first_of(const C1& c1, const C2& c2) {
		return std::find_first_of(begin(c1), end(c1), begin(c2), end(c2));
	}

	/// Searches for any one of a set of elements
	template<typename C1, typename C2, typename F>
	decltype(auto) find_first_of(const C1& c1, const C2& c2, const F& f) {
		return std::find_first_of(begin(c1), end(c1), begin(c2), end(c2), f);
	}

	/// Finds the first two adjacent items that are equal(or satisfy a given predicate)
	template<typename C>
	decltype(auto) adjacent_find(const C& c) {
		return std::adjacent_find(begin(c), end(c));
	}

	/// Finds the first two adjacent items that are equal(or satisfy a given predicate)
	template<typename C, typename F>
	decltype(auto) adjacent_find(const C& c, const F& f) {
		return std::adjacent_find(begin(c), end(c), f);
	}

	/// searches for a range of elements
	template<typename C1, typename C2>
	decltype(auto) search(const C1& c1, const C2& c2) {
		return std::search(begin(c1), end(c1), begin(c2), end(c2));
	}

	/// Searches for a range of elements
	template<typename C1, typename C2, typename F>
	decltype(auto) search(const C1& c1, const C2& c2, const F& f) {
		return std::search(begin(c1), end(c1), begin(c2), end(c2), f);
	}

	/// Searches for n consecutive elements	
	template<typename C, typename N>
	decltype(auto) search_n(const C& c, N n) {
		return std::search(begin(c), end(c), n);
	}

	/// Searches for n consecutive elements	
	template<typename C, typename N, typename F>
	decltype(auto) search_n(const C& c, N n, const F& f) {
		return std::search(begin(c), end(c), n, f);
	}

	//===============================================
	// Modifying sequence operations

	/// Copies a range of elements to a new location
	template<typename C1, typename C2>
	decltype(auto) copy(const C1& c1, C2& c2) {
		return std::copy(begin(c1), end(c1), begin(c2));
	}

	/// Conditionally copies a range of elements to a new location 	
	template<typename C1, typename C2, typename F>
	decltype(auto) copy_if(const C1& c1, C2& c2, const F& f) {
		return std::copy_if(begin(c1), end(c1), begin(c2), f);
	}

	/// Copies N elements from the source to the destination 
	template<typename C1, typename C2, typename N, typename F>
	decltype(auto) copy_n(const C1& c1, N n, C2& c2) {
		return std::copy_n(begin(c1), end(c1), n, end(c2));
	}

	/// Moves a range of elements to a new location
	template<typename C1, typename C2>
	decltype(auto) move(C1& c1, C2& c2) {
		return std::move(begin(c1), end(c1), begin(c2));
	}

	/// Assigns the given value to every element in a range
	template<typename C, typename T>
	void fill(C& c, const T& x) {
		std::fill(begin(c), end(c), x);
	}

	/// Assigns the given value to N elements in the range
	template<typename C, typename N, typename T>
	void fill_n(C& c, N n, const T& x) {
		std::fill_n(begin(c), end(c), n, x);
	}

	/// Applies a function to transform the elements in the range
	template<typename C, typename F>
	decltype(auto) transform(C& c, F& f) {
		return std::transform(c.begin(), c.end(), c.begin(), f);
	}

	/// Applies a function to transform the elements in the range
	template<typename C, typename C2, typename F>
	decltype(auto) transform(const C& c, C2& c2, F& f) {
		return std::transform(c.begin(), c.end(), c2.begin(), f);
	}

	/// Assigns the results of successive function calls to every element in a range
	template<typename C, typename F>
	void generate(C& c, const F& f) {
		std::generate(begin(c), end(c), f);
	}

	/// Assigns the results of successive function calls to N elements in a range
	template<typename C, typename N, typename F>
	void generate_n(C& c, N n, const F& f) {
		std::generate_n(begin(c), end(c), n, f);
	}

	/// Removes elements that are equal to a value
	template<typename C, typename T>
	decltype(auto) remove(C& c, const T& value) {
		return std::remove(begin(c), end(c), value);
	}

	/// Removes elements satisfying specific criteria
	template<typename C, typename F>
	decltype(auto) remove_if(C& c, const F& f) {
		return std::remove_if(begin(c), end(c), f);
	}

	/// Copies a range of elements removing those that are equal to a value
	template<typename C1, typename C2, typename T>
	decltype(auto) remove_copy(const C1& c1, C2& c2, const T& value) {
		return std::remove_copy(begin(c1), end(c1), begin(c2), value);
	}

	/// Copies a range of elements removing that that satisfying specific criteria
	template<typename C1, typename C2, typename F>
	decltype(auto) remove_copy_if(const C1& c1, C2& c2, const F& f) {
		return std::remove_copy_if(begin(c1), end(c1), begin(c2), f);
	}

	/// Replaces all values satisfying specific criteria with another value
	template<typename C, class T>
	void replace(const C& c, const T& old, const T& val) {
		std::replace(begin(c), end(c), old, val);
	}

	/// Replaces all values satisfying specific criteria with another value
	template<typename C, class F, class T>
	void replace_if(const C& c, const F& f, const T& val) {
		std::replace_if(begin(c), end(c), f, val);
	}

	/// Replaces all values satisfying specific criteria with another value
	template<typename C, typename C2, class T>
	void replace_copy(const C& c, C2& c2, const T& old, const T& val) {
		return std::replace_copy(begin(c), end(c), begin(c2), old, val);
	}

	/// Replaces all values satisfying specific criteria with another value
	template<typename C, typename C2, class F, class T>
	void replace_copy_if(const C& c, C2& c2, const F& f, const T& val) {
		std::replace_if(begin(c), end(c), begin(c2), f, val);
	}

	/// Swaps two ranges of elements
	template<typename C1, typename C2>
	decltype(auto) swap_ranges(C1& c1, C2& c2) {
		return std::swap_ranges(begin(c1), end(c1), begin(c2));
	}

	/// Removes consecutive duplicate elements in a range 
	template<typename C>
	decltype(auto) unique(C& c) {
		return std::unique(begin(c), end(c));
	}

	/// Removes consecutive duplicate elements in a range 
	template<typename C, typename F>
	decltype(auto) unique(C& c, const F& f) {
		return std::unique(begin(c), end(c), f);
	}

	/// Copies elements removing consecutive duplicate elements in a range 
	template<typename C, typename C2>
	decltype(auto) unique_copy(const C& c, C2& c2) {
		return std::unique_copy(begin(c), end(c), begin(c2));
	}

	/// Copies elements removing consecutive duplicate elements in a range 
	template<typename C, typename C2, typename F>
	decltype(auto) unique(const C& c, C2& c2, const F& f) {
		return std::unique(begin(c), end(c), begin(c2), f);
	}

	/// Sorts elements in the collection 
	template<typename C>
	void sort(C& c) {
		return std::sort(begin(c), end(c));
	}

	/// Sorts elements in the collection 
	template<typename C, typename F>
	void sort(C& c, const F& f) {
		return std::sort(begin(c), end(c), f);
	}

	/// Returns true if the elements are sorted in ascending order 
	template<typename C>
	bool is_sorted(const C& c) {
		return std::is_sorted(begin(c), end(c));
	}

	/// Returns true if the elements are sorted in ascending order 
	template<typename C, typename F>
	bool is_sorted(const C& c, const F& f) {
		return std::is_sorted(begin(c), end(c), f);
	}

	/// Returns the smallest element in the range 
	template<typename C>
	decltype(auto) min_element(const C& c) {
		return std::min_element(begin(c), end(c));
	}

	/// Returns the largest element in the range 
	template<typename C>
	decltype(auto) max_element(const C& c) {
		return std::max_element(begin(c), end(c));
	}

	/// Returns the smallest and largest element in the range 
	template<typename C>
	decltype(auto) minmax_element(const C& c) {
		return std::minmax_element(begin(c), end(c));
	}

	/// Returns the smallest element in the range 
	template<typename C, typename F>
	decltype(auto) min_element(const C& c, const F& f) {
		return std::min_element(begin(c), end(c), f);
	}

	/// Returns the largest element in the range 
	template<typename C, typename F>
	decltype(auto) max_element(const C& c, const F& f) {
		return std::max_element(begin(c), end(c), f);
	}

	/// Returns the smallest and largest element in the range 
	template<typename C, typename F>
	decltype(auto) minmax_element(const C& c, const F& f) {
		return std::minmax_element(begin(c), end(c), f);
	}

	/// Determines if two sets of elements are the same 
	template<typename C, typename C2>
	decltype(auto) equal(const C& c, const C2& c2) {
		return std::equal(begin(c), end(c), begin(c2), end(c2));
	}

	/// Determines if two sets of elements are the same 
	template<typename C, typename C2, typename F>
	decltype(auto) equal(const C& c, const C2& c2, const F& f) {
		return std::equal(begin(c), end(c), begin(c2), end(c2), f);
	}

	//================================
	// Numeric operations

	/// Fills a range with sequentially increasing values
	template<typename C, typename T>
	void iota(C& c, const T& val = T()) {
		std::iota(begin(c), end(c), val);
	}

	/// Sums up a range of elements
	template<typename C, typename T>
	decltype(auto) accumulate(const C& c, const T& init = T()) {
		return std::accumulate(begin(c), end(c), init);
	}

	/// Sums up a range of elements
	template<typename C, typename T, typename F>
	decltype(auto) accumulate(const C& c, const T& init, const F& f) {
		return std::accumulate(begin(c), end(c), init, f);
	}

	/// Computes the sum of products of two ranges of elements 
	template<typename C, typename C2, typename T>
	decltype(auto) inner_product(const C& c, const C& c2, const T& init) {
		return std::inner_product(begin(c), end(c), begin(c2), init);
	}

	/// Computes the inner product of two ranges of elements
	template<typename C, typename C2, typename T, typename F1, typename F2>
	decltype(auto) inner_product(const C& c, const C& c2, const T& init, const F1& f1, const F2& f2) {
		return std::inner_product(begin(c), end(c), begin(c), init, f1, f2);
	}

	/// Computes the differences between adjacent elements in a range
	template<typename C, typename C2> 
	decltype(auto) adjacent_difference(const C& c, C2& c2) {
		return std::adjacent_difference(begin(c), end(c), begin(c2));
	}

	/// Computes the differences between adjacent elements in a range
	template<typename C, typename C2, typename F>
	decltype(auto) adjacent_difference(const C& c, C2& c2, const F& f) {
		return std::adjacent_difference(begin(c), end(c), begin(c2), f);
	}
		
	/// Computes the partial sum of a range of elements
	template<typename C, typename C2>
	decltype(auto) partial_sum(const C& c, C2& c2) {
		return std::partial_sum(begin(c), end(c), begin(c2));
	}

	/// Computes the partial sum of a range of elements
	template<typename C, typename C2, typename F>
	decltype(auto) partial_sum(const C& c, C2& c2, const F& f) {
		return std::partial_sum(begin(c), end(c), begin(c2), f);
	}
}