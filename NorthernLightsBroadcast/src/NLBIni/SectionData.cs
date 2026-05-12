using System;
using System.Collections.Generic;

namespace NLBIni;

public class SectionData : ICloneable
{
	private IEqualityComparer<string> _searchComparer;

	private List<string> _leadingComments;

	private List<string> _trailingComments = new List<string>();

	private KeyDataCollection _keyDataCollection;

	private string _sectionName;

	public string SectionName
	{
		get
		{
			return _sectionName;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				_sectionName = value;
			}
		}
	}

	[Obsolete("Do not use this property, use property Comments instead")]
	public List<string> LeadingComments
	{
		get
		{
			return _leadingComments;
		}
		internal set
		{
			_leadingComments = new List<string>(value);
		}
	}

	public List<string> Comments => _leadingComments;

	[Obsolete("Do not use this property, use property Comments instead")]
	public List<string> TrailingComments
	{
		get
		{
			return _trailingComments;
		}
		internal set
		{
			_trailingComments = new List<string>(value);
		}
	}

	public KeyDataCollection Keys
	{
		get
		{
			return _keyDataCollection;
		}
		set
		{
			_keyDataCollection = value;
		}
	}

	public SectionData(string sectionName)
		: this(sectionName, EqualityComparer<string>.Default)
	{
	}

	public SectionData(string sectionName, IEqualityComparer<string> searchComparer)
	{
		_searchComparer = searchComparer;
		if (string.IsNullOrEmpty(sectionName))
		{
			throw new ArgumentException("section name can not be empty");
		}
		_leadingComments = new List<string>();
		_keyDataCollection = new KeyDataCollection(_searchComparer);
		SectionName = sectionName;
	}

	public SectionData(SectionData ori, IEqualityComparer<string> searchComparer = null)
	{
		SectionName = ori.SectionName;
		_searchComparer = searchComparer;
		_leadingComments = new List<string>(ori._leadingComments);
		_keyDataCollection = new KeyDataCollection(ori._keyDataCollection, searchComparer ?? ori._searchComparer);
	}

	public void ClearComments()
	{
		Comments.Clear();
		TrailingComments.Clear();
		Keys.ClearComments();
	}

	public void ClearKeyData()
	{
		Keys.RemoveAllKeys();
	}

	public void Merge(SectionData toMergeSection)
	{
		foreach (string leadingComment in toMergeSection.LeadingComments)
		{
			LeadingComments.Add(leadingComment);
		}
		Keys.Merge(toMergeSection.Keys);
		foreach (string trailingComment in toMergeSection.TrailingComments)
		{
			TrailingComments.Add(trailingComment);
		}
	}

	public object Clone()
	{
		return new SectionData(this);
	}
}
