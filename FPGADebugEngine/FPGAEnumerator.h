#pragma once
#include "FPGADebugEngine_i.h"

#include <vector>
#include <type_traits>
#include <tuple>
#include <ranges>

template<class IEnumerator>
class FPGABaseEnumerator
	: public CComObjectRootEx<CComSingleThreadModel>, 
	public IEnumerator
{
	template<typename T>
	struct func_args;

	template<typename C, typename T, typename... Args>
	struct func_args<T(C::*)(Args...)>
	{
		using args = std::tuple<Args...>;

		template<int i>
		using nth_arg = typename std::tuple_element_t<i, args>; // doesnt work IDK why
	};

	// Deduce element as 2nd argument from IEnumerator::Next
	using IElementPtrPtr = typename std::tuple_element_t<1, typename func_args<decltype(&IEnumerator::Next)>::args>;
	using IElement = std::remove_pointer_t<std::remove_pointer_t<IElementPtrPtr>>;


	std::vector<ATL::CComPtr<IElement>> m_elements;
	typename decltype(m_elements)::iterator m_current;

	typedef ATL::CComObject<FPGABaseEnumerator<IEnumerator>> ComObjectType;
public:

BEGIN_COM_MAP(FPGABaseEnumerator)
	COM_INTERFACE_ENTRY(IEnumerator)
END_COM_MAP()

	void Add(IElement* pElem)
	{
		m_elements.push_back(pElem);
	}
	template <typename Range>
	void Add(Range r)
	{
		m_elements.reserve(m_elements.size() + std::size(r));
		for (auto& it : r)
			m_elements.push_back(static_cast<IElement*>(it)); // static_cast because CComPtr<B> cannot be converted to CComPtr<A>
	}
	HRESULT Next(ULONG celt, IElement** rgelt, ULONG* pceltFetched)
	{
		*pceltFetched = 0;
		while (celt > 0 && m_current != m_elements.end())
		{
			m_current->QueryInterface(rgelt);
			++m_current;
			--celt;
			++(*pceltFetched);
			++rgelt;
		}
		return S_OK;
	}
	HRESULT Skip(ULONG celt)
	{
		for (ULONG i = 0; i < celt; ++i)
			++m_current;
		return S_OK;
	}
	HRESULT Reset(void)
	{
		m_current = m_elements.begin();
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
};

template<class IEnumerator>
using CFPGAEnumerator = ATL::CComObject<FPGABaseEnumerator<IEnumerator>>;