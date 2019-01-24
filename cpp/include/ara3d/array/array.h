/*
	Ara 3d Array Library
	Copyright 2018, Ara 3D, Inc.
	Usage licensed under terms of MIT Licenese
*/
#pragma once

namespace ara3d
{
	// Iterator for accessing of items at fixed byte offsets in memory 
	template<typename T, size_t OffsetN = sizeof(T)>
	class mem_stride_iterator
	{
		typedef T ValueType;

		const char* _data;

		const T& operator*() const { return *(T*)_data; }
		T& operator*() { return *(T*)_data; }
		mem_stride_iterator(const char* data = nullptr) : _data(data) { }
		bool operator==(const mem_stride_iterator iter) const { return _data == iter._data; }
		bool operator!=(const mem_stride_iterator iter) const { return _data != iter._data; }
		mem_stride_iterator& operator++() { _data += OffsetN; return *this; }
		mem_stride_iterator operator++(int) { mem_stride_iterator r = *this; (*this)++; return r; }
		mem_stride_iterator operator+(size_t n) const { return mem_stride_iterator(_data + OffsetN * n); }
		mem_stride_iterator& operator+=(size_t n) { _data += OffsetN * n; return *this; }
		size_t operator-(const mem_stride_iterator& iter) const { return _data - iter._data; }
		const T& operator[](size_t n) const { return *(const T*)(_data + OffsetN * n); }
		T& operator[](size_t n) { return *(T*)(_data + OffsetN * n); }
	};

	// Iterator for read-only access of items at fixed byte offsets in memory 
	template<typename T, size_t OffsetN = sizeof(T)>
	class const_mem_stride_iterator
	{
		typedef T ValueType;

		const char* _data;

		const_mem_stride_iterator(const char* data = nullptr) : _data(data) { }
		const_mem_stride_iterator(mem_stride_iterator<T, OffsetN> other) : _data(other._data) { }
		const T& operator*() const { return *(T*)_data; }
		bool operator==(const const_mem_stride_iterator iter) const { return _data == iter._data; }
		bool operator!=(const const_mem_stride_iterator iter) const { return _data != iter._data; }
		const_mem_stride_iterator& operator++() { _data += OffsetN; return *this; }
		const_mem_stride_iterator operator++(int) { const_mem_stride_iterator r = *this; (*this)++; return r; }
		const_mem_stride_iterator operator+(size_t n) const { return const_mem_stride_iterator(_data + OffsetN * n); }
		const_mem_stride_iterator& operator+=(size_t n) { _data += OffsetN * n; return *this; }
		size_t operator-(const const_mem_stride_iterator& iter) const { return _data - iter._data; }
		const T& operator[](size_t n) const { return *(const T*)(_data + OffsetN * n); }
	};

	// Iterator that generating items as needed using a function 
	template<typename F>
	struct func_array_iterator
	{
		typedef typename F::result_type value_type;
		typedef F func_type;

		F _func;
		size_t _i;

		func_array_iterator(const F& func, size_t i = 0) : _func(func), _i(i) { }
		value_type operator*() const { return _func(_i); }
		bool operator==(const func_array_iterator iter) const { return _i == iter._i; }
		bool operator!=(const func_array_iterator iter) const { return _i != iter._i; }
		func_array_iterator& operator++() { return this->operator+=(1); }
		func_array_iterator operator++(int) { func_array_iterator r = *this; (*this)++; return r; }
		func_array_iterator& operator+=(size_t n) { _i += n; return *this; }
		func_array_iterator operator+(size_t n) const { return func_array_iterator(_func, _i + n); }
		size_t operator-(const func_array_iterator& iter) const { return _i - iter._i; }
		value_type operator[](size_t n) const { return _func(_i + n); }
	};

	// A wrapper around an existing iterator that advances it by N items at a time. 
	// There is no non-const version of this iterator, as it would add complexity to the stride operation
	template<typename IterT>
	struct const_strided_iterator 
	{
		typedef IterT iterator;
		typedef typename iterator::value_type value_type;

		iterator _iter;
		size_t _stride;

		const_strided_iterator(const iterator& iter, size_t stride) : _iter(iter), _stride(stride) { }
		value_type operator*() const { return *_iter; }
		bool operator==(const const_strided_iterator iter) const { return _iter == iter._iter; }
		bool operator!=(const const_strided_iterator iter) const { return _iter != iter._iter; }
		const_strided_iterator& operator++() { _iter += _stride; return *this; }
		const_strided_iterator operator++(int) { const_strided_iterator r = *this; (*this)++; return r; }
		const_strided_iterator& operator+=(size_t n) { _iter += _stride * n; return *this; }
		const_strided_iterator operator+(size_t n) const { return const_strided_iterator(_iter + _stride * n, _stride); }
		size_t operator-(const const_strided_iterator& iter) const { return (_iter - iter._iter) / _stride; }
		value_type operator[](size_t n) const { return _iter[n * _stride]; }
	};

	// The base class of all const array implementations 
	template<
		typename T, 
		typename IterT
	>
	struct const_array_base 
	{
		typedef IterT iterator;
		typedef IterT const_iterator;
		typedef T value_type;
		typedef size_t size_type;

		iterator _iter;
		size_t _size;

		const_array_base(iterator begin, size_t size = 0) : _iter(begin), _size(size) { }
		iterator begin() const { return _iter; }
		iterator end() const { return begin() + size(); }
		const value_type& operator[](size_t n) const { return begin()[n]; }
		size_type size() const { return _size; }
		bool empty() const { return size() == 0; }
	};

	// The base class of all array implementations 
	template<
		typename T, 
		typename IterT, 
		typename ConstIterT
	> 
	struct array_base 
	{
		typedef ConstIterT const_iterator;
		typedef IterT iterator;
		typedef T value_type;
		typedef size_t size_type;

		iterator _iter;
		size_t _size;

		array_base(iterator begin = iterator(), size_t size = 0) : _iter(begin), _size(size) { }

		iterator begin() { return _iter; }
		iterator end() { return begin() + size(); }
		const_iterator begin() const { return _iter; }
		const_iterator end() const { return begin() + size(); }
		value_type& operator[](size_t n) { return begin()[n]; }
		const value_type& operator[](size_t n) const { return begin()[n]; }
		size_type size() const { return _size; }
		bool empty() const { return size() == 0; }
	};

	// Represents N elements from any array compatible type (works with vectors)
	template<
		typename ArrayT, 
		typename ValueT = typename ArrayT::value_type, 
		typename IterT = typename ArrayT::iterator, 
		typename ConstIterT = typename ArrayT::const_iterator, 
		typename BaseT = array_base<ValueT, IterT, ConstIterT>
	>
	struct array_slice : public BaseT
	{
		array_slice(IterT begin = IterT(), size_t size = 0) : BaseT(begin, size) { }
	};

	// Represents N mutable elements from any array compatible type (works with vectors)
	template<
		typename ArrayT, 
		typename ValueT = typename ArrayT::value_type, 
		typename IterT = typename ArrayT::iterator, 
		typename BaseT = const_array_base<ValueT, IterT>
	>
	struct const_array_slice : public BaseT
	{
		const_array_slice(IterT begin = IterT(), size_t size = 0) : BaseT(begin, size) { }
	};

	// Strides over elements in an array. 	
	template<
		typename ArrayT, 
		typename ValueT = typename ArrayT::value_type, 
		typename IterT = const_strided_iterator<typename ArrayT::iterator>, 
		typename BaseT = const_array_base<ValueT, IterT>
	>
	struct const_array_stride : public BaseT
	{
		const_array_stride(typename ArrayT::iterator begin = ArrayT::iterator(), size_t size = 0, size_t stride = 0) : const_array_base(IterT(begin, stride), size) { }
	};

	// A mutable view into a contiguous buffer of data without ownership semantics and which can be indexed and sliced. 
	template<
		typename ValueT, 
		typename IterT = ValueT*, 
		typename BaseT = array_base<ValueT, IterT, const ValueT*>
	>
	struct array_view : public BaseT
	{
		array_view(IterT begin = IterT(), size_t size = 0) : BaseT(begin, size) { }
	};

	// A non-mutable view into a contiguous buffer of data of that has no ownership semantics but which can be indexed and sliced. 
	template<
		typename ValueT, 
		typename IterT = const ValueT* , 
		typename BaseT = const_array_base<ValueT, IterT>
	>
	struct const_array_view : public BaseT
	{
		const_array_view(IterT begin = IterT(), size_t size = 0) : BaseT(begin, size) { }
	};

	// An immutable view of a set of values that are in a contigous block of memory, but offset from each other a fixed number of bytes	
	template<
		typename ValueT, size_t OffsetN, 
		typename IterT = const_mem_stride_iterator <ValueT , OffsetN > , 
		typename BaseT = const_array_base<ValueT, IterT >
	>
	struct const_array_mem_stride : public BaseT
	{
		const_array_mem_stride(const ValueT* begin = nullptr, size_t size = 0) : BaseT(IterT(begin), size) { }
	};

	// An mutable view of a set of values that are in a contigous block of memory, but offset from each other an arbitrary number of bytes
	template<
		typename ValueT, 
		size_t OffsetN, 
		typename IterT = mem_stride_iterator <ValueT, OffsetN >, 
		typename ConstIterT = const_mem_stride_iterator <ValueT, OffsetN >, 
		typename BaseT = array_base<ValueT, IterT, ConstIterT >
	>
	struct array_mem_stride : public BaseT, ConstIterT
	{
		array_mem_stride(const ValueT* begin = nullptr, size_t size = 0) : BaseT(IterT(begin), size) { }
	};
	
	// Provides an array interface around a function and a size. Requires functors or std::function to work. 
	template<typename F, typename ValueT = typename F::result_type, typename IterT = func_array_iterator<F>, typename BaseT = const_array_base<ValueT, IterT>>
	struct func_array : public BaseT 
	{	
		func_array(F func, size_t size) : BaseT(IterT(func), size) { }
	};

	// An array container (owns memory) with a run-time defined size. 
	template<typename T, typename BaseT = array_view<T>>
	struct array : public BaseT
	{
		array(size_t size = 0) : BaseT(new T[size], size) { }	
		~array() { delete[] BaseT::begin(); }
	};

	// An array of bytes 
	typedef array<unsigned char> buffer;
}