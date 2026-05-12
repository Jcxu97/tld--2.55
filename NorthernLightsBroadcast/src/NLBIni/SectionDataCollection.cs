using System;
using System.Collections;
using System.Collections.Generic;

namespace NLBIni;

public class SectionDataCollection : ICloneable, IEnumerable<SectionData>, IEnumerable
{
	private IEqualityComparer<string> _searchComparer;

	private readonly Dictionary<string, SectionData> _sectionData;

	public int Count => _sectionData.Count;

	public KeyDataCollection this[string sectionName]
	{
		get
		{
			if (_sectionData.ContainsKey(sectionName))
			{
				return _sectionData[sectionName].Keys;
			}
			return null;
		}
	}

	public SectionDataCollection()
		: this(EqualityComparer<string>.Default)
	{
	}

	public SectionDataCollection(IEqualityComparer<string> searchComparer)
	{
		_searchComparer = searchComparer;
		_sectionData = new Dictionary<string, SectionData>(_searchComparer);
	}

	public SectionDataCollection(SectionDataCollection ori, IEqualityComparer<string> searchComparer)
	{
		_searchComparer = searchComparer ?? EqualityComparer<string>.Default;
		_sectionData = new Dictionary<string, SectionData>(_searchComparer);
		foreach (SectionData item in ori)
		{
			_sectionData.Add(item.SectionName, (SectionData)item.Clone());
		}
	}

	public bool AddSection(string keyName)
	{
		if (!ContainsSection(keyName))
		{
			_sectionData.Add(keyName, new SectionData(keyName, _searchComparer));
			return true;
		}
		return false;
	}

	public void Add(SectionData data)
	{
		if (ContainsSection(data.SectionName))
		{
			SetSectionData(data.SectionName, new SectionData(data, _searchComparer));
		}
		else
		{
			_sectionData.Add(data.SectionName, new SectionData(data, _searchComparer));
		}
	}

	public void Clear()
	{
		_sectionData.Clear();
	}

	public bool ContainsSection(string keyName)
	{
		return _sectionData.ContainsKey(keyName);
	}

	public SectionData GetSectionData(string sectionName)
	{
		if (_sectionData.ContainsKey(sectionName))
		{
			return _sectionData[sectionName];
		}
		return null;
	}

	public void Merge(SectionDataCollection sectionsToMerge)
	{
		foreach (SectionData item in sectionsToMerge)
		{
			if (GetSectionData(item.SectionName) == null)
			{
				AddSection(item.SectionName);
			}
			this[item.SectionName].Merge(item.Keys);
		}
	}

	public void SetSectionData(string sectionName, SectionData data)
	{
		if (data != null)
		{
			_sectionData[sectionName] = data;
		}
	}

	public bool RemoveSection(string keyName)
	{
		return _sectionData.Remove(keyName);
	}

	public IEnumerator<SectionData> GetEnumerator()
	{
		foreach (string key in _sectionData.Keys)
		{
			yield return _sectionData[key];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public object Clone()
	{
		return new SectionDataCollection(this, _searchComparer);
	}
}
