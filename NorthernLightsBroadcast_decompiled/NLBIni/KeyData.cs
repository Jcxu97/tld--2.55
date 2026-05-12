using System;
using System.Collections.Generic;

namespace NLBIni;

public class KeyData : ICloneable
{
	private List<string> _comments;

	private string _value;

	private string _keyName;

	public List<string> Comments
	{
		get
		{
			return _comments;
		}
		set
		{
			_comments = new List<string>(value);
		}
	}

	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public string KeyName
	{
		get
		{
			return _keyName;
		}
		set
		{
			if (value != string.Empty)
			{
				_keyName = value;
			}
		}
	}

	public KeyData(string keyName)
	{
		if (string.IsNullOrEmpty(keyName))
		{
			throw new ArgumentException("key name can not be empty");
		}
		_comments = new List<string>();
		_value = string.Empty;
		_keyName = keyName;
	}

	public KeyData(KeyData ori)
	{
		_value = ori._value;
		_keyName = ori._keyName;
		_comments = new List<string>(ori._comments);
	}

	public object Clone()
	{
		return new KeyData(this);
	}
}
