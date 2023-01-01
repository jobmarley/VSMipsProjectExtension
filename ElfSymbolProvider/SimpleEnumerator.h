#pragma once
#include "framework.h"


template<class IEnumerator>
class SimpleEnumerator
	: public CComObjectRootEx<CComObjectThreadModel>,
	public IEnumerator
{
	template<typename T>
	struct func_args;

	// This allows to access the argument types from a function pointer type
	template<typename C, typename T, typename... Args>
	struct func_args<T(C::*)(Args...)>
	{
		using args = std::tuple<Args...>;

		template<int i>
		using nth_arg = typename std::tuple_element_t<i, args>; // doesnt work IDK why
	};

	// Deduce element as 2nd argument from IEnumerator::Next
	using IElementPtr = typename std::tuple_element_t<1, typename func_args<decltype(&IEnumerator::Next)>::args>;
	// This is IUnknown*, DEBUG_PROPERTY_INFO, etc...
	using IElement = std::remove_pointer_t<IElementPtr>;

	// Sometimes we have IUnknown* derived stuff, in which case we have to wrap them in CComPtr
	// other times we have structures, and we should just keep them in vector
	template<typename T>
	struct Wrapper
	{
		using Type = T;
	};
	template<typename T>
	struct Wrapper<T*>
	{
		using Type = ATL::CComPtr<T>;
	};
	using WrappedElement = Wrapper<IElement>::Type;
	std::vector<WrappedElement> m_elements;
	size_t m_currentIndex = 0;

	typedef ATL::CComObject<SimpleEnumerator<IEnumerator>> ComObjectType;

	inline void CopyElement(const IElement& from, IElement* to)
	{
		static_assert(std::is_pointer_v<IElement> == false); // safety check
		*to = from;
	}
	inline void CopyElement(const ATL::CComPtr<std::remove_pointer_t<IElement>>& from, IElement* to)
	{
		from.QueryInterface(to);
	}
public:
	SimpleEnumerator()
	{
	}
	BEGIN_COM_MAP(SimpleEnumerator)
		COM_INTERFACE_ENTRY(IEnumerator)
	END_COM_MAP()

	void Add(IElement pElem)
	{
		m_elements.push_back(pElem);
	}
	template <typename Range>
	void Add(Range r)
	{
		m_elements.reserve(m_elements.size() + std::size(r));
		for (const auto& it : r)
			m_elements.push_back(static_cast<IElement>(it)); // static_cast because CComPtr<B> cannot be converted to CComPtr<A>
	}
	HRESULT Next(ULONG celt, IElement* rgelt, ULONG* pceltFetched)
	{
		*pceltFetched = 0;
		while (celt > 0 && m_currentIndex < m_elements.size())
		{
			CopyElement(m_elements[m_currentIndex], rgelt);
			++m_currentIndex;
			--celt;
			++(*pceltFetched);
			++rgelt;
		}
		return S_OK;
	}
	HRESULT Skip(ULONG celt)
	{
		for (ULONG i = 0; i < celt; ++i)
			++m_currentIndex;
		return S_OK;
	}
	HRESULT Reset(void)
	{
		m_currentIndex = 0;
		return S_OK;
	}
	HRESULT Clone(IEnumerator** ppEnum)
	{
		ATL::CComPtr<ComObjectType> p;
		HRESULT hr = ComObjectType::CreateInstance(&p);
		if (FAILED(hr))
			return hr;

		return p->QueryInterface(&ppEnum);
	}
	HRESULT GetCount(ULONG* pcelt)
	{
		*pcelt = (ULONG)m_elements.size();
		return S_OK;
	}

	template <typename Range>
	static HRESULT Create(Range r, IEnumerator** ppEnumerator)
	{
		CComPtr<CSimpleEnumerator<IEnumerator>> pEnumerator;
		// this doesnt call SimpleEnumerator::CreateInstance but CComObject::CreateInstance
		HRESULT hr = CSimpleEnumerator<IEnumerator>::CreateInstance(&pEnumerator);
		if (FAILED(hr))
			return hr;

		pEnumerator->Add(r);
		pEnumerator.QueryInterface(ppEnumerator);
		pEnumerator.Detach(); // CComObject, as opposed to CComCoClass, doesnt AddRef, so we have to detach
		return S_OK;
	}
};

template<class IEnumerator>
using CSimpleEnumerator = ATL::CComObject<SimpleEnumerator<IEnumerator>>;