using System;
using System.Collections;
using System.Collections.Generic;

namespace NLBIni;

public class KeyDataCollection : ICloneable, IEnumerable<KeyData>, IEnumerable
{
	private IEqualityComparer<string> _searchComparer;

	private readonly Dictionary<string, KeyData> _keyData;

	public string this[string keyName]
	{
		get
		{
			if (_keyData.ContainsKey(keyName))
			{
				return _keyData[keyName].Value;
			}
			return null;
		}
		set
		{
			if (!_keyData.ContainsKey(keyName))
			{
				AddKey(keyName);
			}
			_keyData[keyName].Value = value;
		}
	}

	public int Count => _keyData.Count;

	public KeyDataCollection()
		: this(EqualityComparer<string>.Default)
	{
	}

	public KeyDataCollection(IEqualityComparer<string> searchComparer)
	{
		_searchComparer = searchComparer;
		_keyData = new Dictionary<string, KeyData>(_searchComparer);
	}

	public KeyDataCollection(KeyDataCollection ori, IEqualityComparer<string> searchComparer)
		: this(searchComparer)
	{
		foreach (KeyData item in ori)
		{
			if (_keyData.ContainsKey(item.KeyName))
			{
				_keyData[item.KeyName] = (KeyData)item.Clone();
			}
			else
			{
				_keyData.Add(item.KeyName, (KeyData)item.Clone());
			}
		}
	}

	public bool AddKey(string keyName)
	{
		if (!_keyData.ContainsKey(keyName))
		{
			_keyData.Add(keyName, new KeyData(keyName));
			return true;
		}
		return false;
	}

	[Obsolete("Pottentially buggy method! Use AddKey(KeyData keyData) instead (See comments in code for an explanation of the bug)")]
	public bool AddKey(string keyName, KeyData keyData)
	{
		if (AddKey(keyName))
		{
			_keyData[keyName] = keyData;
			return true;
		}
		return false;
	}

	public bool AddKey(KeyData keyData)
	{
		if (AddKey(keyData.KeyName))
		{
			_keyData[keyData.KeyName] = keyData;
			return true;
		}
		return false;
	}

	public bool AddKey(string keyName, string keyValue)
	{
		if (AddKey(keyName))
		{
			_keyData[keyName].Value = keyValue;
			return true;
		}
		return false;
	}

	public void ClearComments()
	{
		using IEnumerator<KeyData> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Comments.Clear();
		}
	}

	public bool ContainsKey(string keyName)
	{
		return _keyData.ContainsKey(keyName);
	}

	public KeyData GetKeyData(string keyName)
	{
		if (_keyData.ContainsKey(keyName))
		{
			return _keyData[keyName];
		}
		return null;
	}

	public void Merge(KeyDataCollection keyDataToMerge)
	{
		foreach (KeyData item in keyDataToMerge)
		{
			AddKey(item.KeyName);
			GetKeyData(item.KeyName).Comments.AddRange(item.Comments);
			this[item.KeyName] = item.Value;
		}
	}

	public void RemoveAllKeys()
	{
		_keyData.Clear();
	}

	public bool RemoveKey(string keyName)
	{
		return _keyData.Remove(keyName);
	}

	public void SetKeyData(KeyData data)
	{
		if (data != null)
		{
			if (_keyData.ContainsKey(data.KeyName))
			{
				RemoveKey(data.KeyName);
			}
			AddKey(data);
		}
	}

	public IEnumerator<KeyData> GetEnumerator()
	{
		foreach (string key in _keyData.Keys)
		{
			yield return _keyData[key];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _keyData.GetEnumerator();
	}

	public object Clone()
	{
		return new KeyDataCollection(this, _searchComparer);
	}

	internal KeyData GetLast()
	{
		KeyData result = null;
		if (_keyData.Keys.Count <= 0)
		{
			return result;
		}
		foreach (string key in _keyData.Keys)
		{
			result = _keyData[key];
		}
		return result;
	}
}
