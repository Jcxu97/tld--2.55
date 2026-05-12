using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using Microsoft.CodeAnalysis;
using ModSettings;
using PastimeReading;
using PastimeReadingRTL;
using UnityEngine;
using UnityEngine.UI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("PastimeReading")]
[assembly: AssemblyCopyright("Created by Waltz")]
[assembly: AssemblyFileVersion("1.2.4")]
[assembly: MelonInfo(typeof(ReadMain), "PastimeReading", "1.2.4", "Waltz", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]
[assembly: TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName = ".NET 6.0")]
[assembly: AssemblyVersion("1.2.4.0")]
[module: RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace PastimeReadingRTL
{
	public static class Char32Utils
	{
		public static bool IsUnicode16Char(int ch)
		{
			return ch < 65535;
		}

		public static bool IsRTLCharacter(int ch)
		{
			if (!IsUnicode16Char(ch))
			{
				return false;
			}
			return TextUtils.IsRTLCharacter((char)ch);
		}

		public static bool IsEnglishLetter(int ch)
		{
			if (!IsUnicode16Char(ch))
			{
				return false;
			}
			return TextUtils.IsEnglishLetter((char)ch);
		}

		public static bool IsNumber(int ch, bool preserveNumbers, bool farsi)
		{
			if (!IsUnicode16Char(ch))
			{
				return false;
			}
			return TextUtils.IsNumber((char)ch, preserveNumbers, farsi);
		}

		public static bool IsSymbol(int ch)
		{
			if (!IsUnicode16Char(ch))
			{
				return false;
			}
			return char.IsSymbol((char)ch);
		}

		public static bool IsLetter(int ch)
		{
			if (!IsUnicode16Char(ch))
			{
				return false;
			}
			return char.IsLetter((char)ch);
		}

		public static bool IsPunctuation(int ch)
		{
			if (!IsUnicode16Char(ch))
			{
				return false;
			}
			return char.IsPunctuation((char)ch);
		}

		public static bool IsWhiteSpace(int ch)
		{
			if (!IsUnicode16Char(ch))
			{
				return false;
			}
			return char.IsWhiteSpace((char)ch);
		}
	}
	public enum ArabicGeneralLetters
	{
		Hamza = 1569,
		Alef = 1575,
		AlefHamza = 1571,
		WawHamza = 1572,
		AlefMaksoor = 1573,
		AlefMaksura = 1609,
		HamzaNabera = 1574,
		Ba = 1576,
		Ta = 1578,
		Tha2 = 1579,
		Jeem = 1580,
		H7aa = 1581,
		Khaa2 = 1582,
		Dal = 1583,
		Thal = 1584,
		Ra2 = 1585,
		Zeen = 1586,
		Seen = 1587,
		Sheen = 1588,
		S9a = 1589,
		Dha = 1590,
		T6a = 1591,
		T6ha = 1592,
		Ain = 1593,
		Gain = 1594,
		Fa = 1601,
		Gaf = 1602,
		Kaf = 1603,
		Lam = 1604,
		Meem = 1605,
		Noon = 1606,
		Ha = 1607,
		Waw = 1608,
		Ya = 1610,
		AlefMad = 1570,
		TaMarboota = 1577,
		PersianYa = 1740,
		PersianPe = 1662,
		PersianChe = 1670,
		PersianZe = 1688,
		PersianGaf = 1711,
		PersianGaf2 = 1705,
		ArabicTatweel = 1600,
		ZeroWidthNoJoiner = 8204
	}
	internal enum ArabicIsolatedLetters
	{
		Hamza = 65152,
		Alef = 65165,
		AlefHamza = 65155,
		WawHamza = 65157,
		AlefMaksoor = 65159,
		AlefMaksura = 65263,
		HamzaNabera = 65161,
		Ba = 65167,
		Ta = 65173,
		Tha2 = 65177,
		Jeem = 65181,
		H7aa = 65185,
		Khaa2 = 65189,
		Dal = 65193,
		Thal = 65195,
		Ra2 = 65197,
		Zeen = 65199,
		Seen = 65201,
		Sheen = 65205,
		S9a = 65209,
		Dha = 65213,
		T6a = 65217,
		T6ha = 65221,
		Ain = 65225,
		Gain = 65229,
		Fa = 65233,
		Gaf = 65237,
		Kaf = 65241,
		Lam = 65245,
		Meem = 65249,
		Noon = 65253,
		Ha = 65257,
		Waw = 65261,
		Ya = 65265,
		AlefMad = 65153,
		TaMarboota = 65171,
		PersianYa = 64508,
		PersianPe = 64342,
		PersianChe = 64378,
		PersianZe = 64394,
		PersianGaf = 64402,
		PersianGaf2 = 64398
	}
	public enum EnglishNumbers
	{
		Zero = 48,
		One,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine
	}
	public enum FarsiNumbers
	{
		Zero = 1776,
		One,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine
	}
	public enum HinduNumbers
	{
		Zero = 1632,
		One,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine
	}
	public enum TashkeelCharacters
	{
		Fathan = 1611,
		Dammatan = 1612,
		Kasratan = 1613,
		Fatha = 1614,
		Damma = 1615,
		Kasra = 1616,
		Shadda = 1617,
		Sukun = 1618,
		MaddahAbove = 1619,
		SuperscriptAlef = 1648,
		ShaddaWithDammatanIsolatedForm = 64606,
		ShaddaWithKasratanIsolatedForm = 64607,
		ShaddaWithFathaIsolatedForm = 64608,
		ShaddaWithDammaIsolatedForm = 64609,
		ShaddaWithKasraIsolatedForm = 64610,
		ShaddaWithSuperscriptAlefIsolatedForm = 64611
	}
	public class FastStringBuilder
	{
		private int length;

		private int[] array;

		private int capacity;

		public int Length
		{
			get
			{
				return length;
			}
			set
			{
				if (value <= length)
				{
					length = value;
				}
			}
		}

		public FastStringBuilder(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}
			this.capacity = capacity;
			array = new int[capacity];
		}

		public FastStringBuilder(string text)
			: this(text, text.Length)
		{
		}

		public FastStringBuilder(string text, int capacity)
			: this(capacity)
		{
			SetValue(text);
		}

		public static implicit operator string(FastStringBuilder x)
		{
			return x.ToString();
		}

		public static implicit operator FastStringBuilder(string x)
		{
			return new FastStringBuilder(x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Get(int index)
		{
			return array[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int index, int ch)
		{
			array[index] = ch;
		}

		public void SetValue(string text)
		{
			int num = 0;
			length = text.Length;
			EnsureCapacity(length, keepValues: false);
			for (int i = 0; i < text.Length; i++)
			{
				int num2 = char.ConvertToUtf32(text, i);
				if (num2 > 65535)
				{
					i++;
				}
				array[num++] = num2;
			}
			length = num;
		}

		public void SetValue(FastStringBuilder other)
		{
			EnsureCapacity(other.length, keepValues: false);
			Copy(other.array, array);
			length = other.length;
		}

		public void Append(int ch)
		{
			length++;
			if (capacity < length)
			{
				EnsureCapacity(length, keepValues: true);
			}
			array[length - 1] = ch;
		}

		public void Append(char ch)
		{
			length++;
			if (capacity < length)
			{
				EnsureCapacity(length, keepValues: true);
			}
			array[length - 1] = ch;
		}

		public void Insert(int pos, FastStringBuilder str, int offset, int count)
		{
			if (str == this)
			{
				throw new InvalidOperationException("You cannot pass the same string builder to insert");
			}
			if (count != 0)
			{
				length += count;
				EnsureCapacity(length, keepValues: true);
				for (int num = length - count - 1; num >= pos; num--)
				{
					array[num + count] = array[num];
				}
				for (int i = 0; i < count; i++)
				{
					array[pos + i] = str.array[offset + i];
				}
			}
		}

		public void Insert(int pos, FastStringBuilder str)
		{
			Insert(pos, str, 0, str.length);
		}

		public void Insert(int pos, int ch)
		{
			length++;
			EnsureCapacity(length, keepValues: true);
			for (int num = length - 2; num >= pos; num--)
			{
				array[num + 1] = array[num];
			}
			array[pos] = ch;
		}

		public void RemoveAll(int character)
		{
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				if (array[i] != character)
				{
					array[num] = array[i];
					num++;
				}
			}
			length = num;
		}

		public void Remove(int start, int length)
		{
			for (int i = start; i < this.length - length; i++)
			{
				array[i] = array[i + length];
			}
			this.length -= length;
		}

		public void Reverse(int startIndex, int length)
		{
			for (int i = 0; i < length / 2; i++)
			{
				int num = startIndex + i;
				int num2 = startIndex + length - i - 1;
				int num3 = array[num];
				int num4 = array[num2];
				array[num] = num4;
				array[num2] = num3;
			}
		}

		public void Reverse()
		{
			Reverse(0, length);
		}

		public void Substring(FastStringBuilder output, int start, int length)
		{
			output.length = 0;
			for (int i = 0; i < length; i++)
			{
				output.Append(array[start + i]);
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(char.ConvertFromUtf32(array[i]));
			}
			return stringBuilder.ToString();
		}

		public void Replace(int oldChar, int newChar)
		{
			for (int i = 0; i < length; i++)
			{
				if (array[i] == oldChar)
				{
					array[i] = newChar;
				}
			}
		}

		public void Replace(FastStringBuilder oldStr, FastStringBuilder newStr)
		{
			for (int i = 0; i < length; i++)
			{
				bool flag = true;
				for (int j = 0; j < oldStr.Length; j++)
				{
					if (array[i + j] != oldStr.Get(j))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (oldStr.Length == newStr.Length)
				{
					for (int k = 0; k < oldStr.Length; k++)
					{
						array[i + k] = newStr.Get(k);
					}
				}
				else if (oldStr.Length < newStr.Length)
				{
					int num = newStr.Length - oldStr.Length;
					length += num;
					EnsureCapacity(length, keepValues: true);
					for (int num2 = length - num - 1; num2 >= i + oldStr.Length; num2--)
					{
						array[num2 + num] = array[num2];
					}
					for (int l = 0; l < newStr.Length; l++)
					{
						array[i + l] = newStr.Get(l);
					}
				}
				else
				{
					int num3 = oldStr.Length - newStr.Length;
					for (int m = i + num3; m < length - num3; m++)
					{
						array[m] = array[m + num3];
					}
					for (int n = 0; n < newStr.Length; n++)
					{
						array[i + n] = newStr.Get(n);
					}
					length -= num3;
				}
				i += newStr.Length;
			}
		}

		public void Clear()
		{
			length = 0;
		}

		private void EnsureCapacity(int cap, bool keepValues)
		{
			if (capacity < cap)
			{
				if (capacity == 0)
				{
					capacity = 1;
				}
				while (capacity < cap)
				{
					capacity *= 2;
				}
				if (keepValues)
				{
					int[] dst = new int[capacity];
					Copy(array, dst);
					array = dst;
				}
				else
				{
					array = new int[capacity];
				}
			}
		}

		private static void Copy(int[] src, int[] dst)
		{
			for (int i = 0; i < src.Length; i++)
			{
				dst[i] = src[i];
			}
		}
	}
	public static class GlyphTable
	{
		private static readonly Dictionary<char, char> MapList;

		static GlyphTable()
		{
			string[] names = Enum.GetNames(typeof(ArabicIsolatedLetters));
			MapList = new Dictionary<char, char>(names.Length);
			string[] array = names;
			foreach (string value in array)
			{
				MapList.Add((char)(int)Enum.Parse(typeof(ArabicGeneralLetters), value), (char)(int)Enum.Parse(typeof(ArabicIsolatedLetters), value));
			}
		}

		public static char Convert(char toBeConverted)
		{
			char value;
			return MapList.TryGetValue(toBeConverted, out value) ? value : toBeConverted;
		}
	}
	public static class GlyphFixer
	{
		public static Dictionary<char, char> EnglishToFarsiNumberMap = new Dictionary<char, char>
		{
			['0'] = '۰',
			['1'] = '۱',
			['2'] = '۲',
			['3'] = '۳',
			['4'] = '۴',
			['5'] = '۵',
			['6'] = '۶',
			['7'] = '۷',
			['8'] = '۸',
			['9'] = '۹'
		};

		public static Dictionary<char, char> EnglishToHinduNumberMap = new Dictionary<char, char>
		{
			['0'] = '٠',
			['1'] = '١',
			['2'] = '٢',
			['3'] = '٣',
			['4'] = '٤',
			['5'] = '٥',
			['6'] = '٦',
			['7'] = '٧',
			['8'] = '٨',
			['9'] = '٩'
		};

		public static void Fix(FastStringBuilder input, FastStringBuilder output, bool preserveNumbers, bool farsi, bool fixTextTags)
		{
			FixYah(input, farsi);
			output.SetValue(input);
			for (int i = 0; i < input.Length; i++)
			{
				bool flag = false;
				int num = input.Get(i);
				if (num == 1604 && i < input.Length - 1)
				{
					flag = HandleSpecialLam(input, output, i);
					if (flag)
					{
						num = output.Get(i);
					}
				}
				if (num == 1600 || num == 8204)
				{
					continue;
				}
				if (num < 65535 && TextUtils.IsGlyphFixedArabicCharacter((char)num))
				{
					char c = GlyphTable.Convert((char)num);
					if (IsMiddleLetter(input, i))
					{
						output.Set(i, (ushort)(c + 3));
					}
					else if (IsFinishingLetter(input, i))
					{
						output.Set(i, (ushort)(c + 1));
					}
					else if (IsLeadingLetter(input, i))
					{
						output.Set(i, (ushort)(c + 2));
					}
				}
				if (flag)
				{
					i++;
				}
			}
			if (!preserveNumbers)
			{
				if (fixTextTags)
				{
					FixNumbersOutsideOfTags(output, farsi);
				}
				else
				{
					FixNumbers(output, farsi);
				}
			}
		}

		public static void FixYah(FastStringBuilder text, bool farsi)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (farsi && text.Get(i) == 1610)
				{
					text.Set(i, 1740);
				}
				else if (!farsi && text.Get(i) == 1740)
				{
					text.Set(i, 1610);
				}
			}
		}

		private static bool HandleSpecialLam(FastStringBuilder input, FastStringBuilder output, int i)
		{
			bool flag;
			switch (input.Get(i + 1))
			{
			case 1573:
				output.Set(i, 65271);
				flag = true;
				break;
			case 1575:
				output.Set(i, 65273);
				flag = true;
				break;
			case 1571:
				output.Set(i, 65269);
				flag = true;
				break;
			case 1570:
				output.Set(i, 65267);
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				output.Set(i + 1, 65535);
			}
			return flag;
		}

		public static void FixNumbers(FastStringBuilder text, bool farsi)
		{
			text.Replace(48, farsi ? 1776 : 1632);
			text.Replace(49, farsi ? 1777 : 1633);
			text.Replace(50, farsi ? 1778 : 1634);
			text.Replace(51, farsi ? 1779 : 1635);
			text.Replace(52, farsi ? 1780 : 1636);
			text.Replace(53, farsi ? 1781 : 1637);
			text.Replace(54, farsi ? 1782 : 1638);
			text.Replace(55, farsi ? 1783 : 1639);
			text.Replace(56, farsi ? 1784 : 1640);
			text.Replace(57, farsi ? 1785 : 1641);
		}

		public static void FixNumbersOutsideOfTags(FastStringBuilder text, bool farsi)
		{
			HashSet<char> hashSet = new HashSet<char>(EnglishToFarsiNumberMap.Keys);
			for (int i = 0; i < text.Length; i++)
			{
				int num = text.Get(i);
				if (num == 60)
				{
					bool flag = false;
					for (int j = i + 1; j < text.Length; j++)
					{
						int num2 = text.Get(j);
						if ((j == i + 1 && num2 == 32) || num2 == 60)
						{
							break;
						}
						if (num2 == 62)
						{
							i = j;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				if (hashSet.Contains((char)num))
				{
					text.Set(i, farsi ? EnglishToFarsiNumberMap[(char)num] : EnglishToHinduNumberMap[(char)num]);
				}
			}
		}

		private static bool IsLeadingLetter(FastStringBuilder letters, int index)
		{
			int num = letters.Get(index);
			int num2 = 0;
			if (index != 0)
			{
				num2 = letters.Get(index - 1);
			}
			int num3 = 0;
			if (index < letters.Length - 1)
			{
				num3 = letters.Get(index + 1);
			}
			bool flag = index == 0 || (num2 < 65535 && !TextUtils.IsGlyphFixedArabicCharacter((char)num2)) || num2 == 1575 || num2 == 1583 || num2 == 1584 || num2 == 1585 || num2 == 1586 || num2 == 1688 || num2 == 1608 || num2 == 1570 || num2 == 1571 || num2 == 1569 || num2 == 1573 || num2 == 8204 || num2 == 1572 || num2 == 65165 || num2 == 65193 || num2 == 65195 || num2 == 65197 || num2 == 65199 || num2 == 64394 || num2 == 65261 || num2 == 65153 || num2 == 65155 || num2 == 65152 || num2 == 65159;
			bool flag2 = num != 32 && num != 1583 && num != 1584 && num != 1585 && num != 1586 && num != 1688 && num != 1575 && num != 1571 && num != 1573 && num != 1570 && num != 1572 && num != 1608 && num != 8204 && num != 1569;
			bool flag3 = index < letters.Length - 1 && num3 < 65535 && TextUtils.IsGlyphFixedArabicCharacter((char)num3) && num3 != 1569 && num3 != 8204;
			return flag && flag2 && flag3;
		}

		private static bool IsFinishingLetter(FastStringBuilder letters, int index)
		{
			int num = letters.Get(index);
			int num2 = 0;
			if (index != 0)
			{
				num2 = letters.Get(index - 1);
			}
			bool flag = index != 0 && num2 != 32 && num2 != 1583 && num2 != 1584 && num2 != 1585 && num2 != 1586 && num2 != 1688 && num2 != 1608 && num2 != 1575 && num2 != 1570 && num2 != 1571 && num2 != 1573 && num2 != 1572 && num2 != 1569 && num2 != 8204 && num2 != 65193 && num2 != 65195 && num2 != 65197 && num2 != 65199 && num2 != 64394 && num2 != 65261 && num2 != 65165 && num2 != 65153 && num2 != 65155 && num2 != 65159 && num2 != 65157 && num2 != 65152 && num2 < 65535 && TextUtils.IsGlyphFixedArabicCharacter((char)num2);
			bool flag2 = num != 32 && num != 8204 && num != 1569;
			return flag && flag2;
		}

		private static bool IsMiddleLetter(FastStringBuilder letters, int index)
		{
			int num = letters.Get(index);
			int num2 = 0;
			if (index != 0)
			{
				num2 = letters.Get(index - 1);
			}
			int num3 = 0;
			if (index < letters.Length - 1)
			{
				num3 = letters.Get(index + 1);
			}
			bool flag = index != 0 && num != 1575 && num != 1583 && num != 1584 && num != 1585 && num != 1586 && num != 1688 && num != 1608 && num != 1570 && num != 1571 && num != 1573 && num != 1572 && num != 8204 && num != 1569;
			bool flag2 = index != 0 && num2 != 1575 && num2 != 1583 && num2 != 1584 && num2 != 1585 && num2 != 1586 && num2 != 1688 && num2 != 1608 && num2 != 1570 && num2 != 1571 && num2 != 1573 && num2 != 1572 && num2 != 1569 && num2 != 8204 && num2 != 65165 && num2 != 65193 && num2 != 65195 && num2 != 65197 && num2 != 65199 && num2 != 64394 && num2 != 65261 && num2 != 65153 && num2 != 65155 && num2 != 65159 && num2 != 65157 && num2 != 65152 && num2 < 65535 && TextUtils.IsGlyphFixedArabicCharacter((char)num2);
			bool flag3 = index < letters.Length - 1 && num3 < 65535 && TextUtils.IsGlyphFixedArabicCharacter((char)num3) && num3 != 8204 && num3 != 1569 && num3 != 65152;
			return flag3 && flag2 && flag;
		}
	}
	public static class LigatureFixer
	{
		private static readonly List<int> LtrTextHolder = new List<int>(512);

		private static readonly List<int> TagTextHolder = new List<int>(512);

		private static readonly Dictionary<char, char> MirroredCharsMap = new Dictionary<char, char>
		{
			['('] = ')',
			[')'] = '(',
			['»'] = '«',
			['«'] = '»'
		};

		private static readonly HashSet<char> MirroredCharsSet = new HashSet<char>(MirroredCharsMap.Keys);

		private static void FlushBufferToOutput(List<int> buffer, FastStringBuilder output)
		{
			for (int i = 0; i < buffer.Count; i++)
			{
				output.Append(buffer[buffer.Count - 1 - i]);
			}
			buffer.Clear();
		}

		public static void Fix(FastStringBuilder input, FastStringBuilder output, bool farsi, bool fixTextTags, bool preserveNumbers)
		{
			LtrTextHolder.Clear();
			TagTextHolder.Clear();
			for (int num = input.Length - 1; num >= 0; num--)
			{
				bool flag = num > 0 && num < input.Length - 1;
				bool flag2 = num == 0;
				bool flag3 = num == input.Length - 1;
				int num2 = input.Get(num);
				int ch = 0;
				if (!flag3)
				{
					ch = input.Get(num + 1);
				}
				int ch2 = 0;
				if (!flag2)
				{
					ch2 = input.Get(num - 1);
				}
				if (fixTextTags && num2 == 62)
				{
					bool flag4 = false;
					int num3 = num;
					TagTextHolder.Add(num2);
					for (int num4 = num - 1; num4 >= 0; num4--)
					{
						int num5 = input.Get(num4);
						TagTextHolder.Add(num5);
						if (num5 == 60)
						{
							int num6 = input.Get(num4 + 1);
							if (num6 != 32)
							{
								flag4 = true;
								num3 = num4;
							}
							break;
						}
					}
					if (flag4)
					{
						FlushBufferToOutput(LtrTextHolder, output);
						FlushBufferToOutput(TagTextHolder, output);
						num = num3;
						continue;
					}
					TagTextHolder.Clear();
				}
				if (Char32Utils.IsPunctuation(num2) || Char32Utils.IsSymbol(num2))
				{
					if (MirroredCharsSet.Contains((char)num2))
					{
						bool flag5 = Char32Utils.IsRTLCharacter(ch2);
						bool flag6 = Char32Utils.IsRTLCharacter(ch);
						if (flag5 || flag6)
						{
							num2 = MirroredCharsMap[(char)num2];
						}
					}
					if (flag)
					{
						bool flag7 = Char32Utils.IsRTLCharacter(ch2);
						bool flag8 = Char32Utils.IsRTLCharacter(ch);
						bool flag9 = Char32Utils.IsWhiteSpace(ch);
						bool flag10 = Char32Utils.IsWhiteSpace(ch2);
						bool flag11 = num2 == 95;
						bool flag12 = num2 == 46 || num2 == 1548 || num2 == 1563;
						if ((flag8 && flag7) || (flag10 && flag12) || (flag9 && flag7) || (flag8 && flag10) || ((flag8 || flag7) && flag11))
						{
							FlushBufferToOutput(LtrTextHolder, output);
							output.Append(num2);
						}
						else
						{
							LtrTextHolder.Add(num2);
						}
					}
					else if (flag3)
					{
						LtrTextHolder.Add(num2);
					}
					else if (flag2)
					{
						output.Append(num2);
					}
					continue;
				}
				if (flag)
				{
					bool flag13 = Char32Utils.IsEnglishLetter(ch2);
					bool flag14 = Char32Utils.IsEnglishLetter(ch);
					bool flag15 = Char32Utils.IsNumber(ch2, preserveNumbers, farsi);
					bool flag16 = Char32Utils.IsNumber(ch, preserveNumbers, farsi);
					bool flag17 = Char32Utils.IsSymbol(ch2);
					bool flag18 = Char32Utils.IsSymbol(ch);
					if (num2 == 32 && (flag14 || flag16 || flag18) && (flag13 || flag15 || flag17))
					{
						LtrTextHolder.Add(num2);
						continue;
					}
				}
				if (Char32Utils.IsEnglishLetter(num2) || Char32Utils.IsNumber(num2, preserveNumbers, farsi))
				{
					LtrTextHolder.Add(num2);
					continue;
				}
				if ((num2 >= 55296 && num2 <= 56319) || (num2 >= 56320 && num2 <= 57343))
				{
					LtrTextHolder.Add(num2);
					continue;
				}
				FlushBufferToOutput(LtrTextHolder, output);
				if (num2 != 65535 && num2 != 8204)
				{
					output.Append(num2);
				}
			}
			FlushBufferToOutput(LtrTextHolder, output);
		}
	}
	public static class RTLSupport
	{
		public const int DefaultBufferSize = 2048;

		private static FastStringBuilder inputBuilder;

		private static FastStringBuilder glyphFixerOutput;

		static RTLSupport()
		{
			inputBuilder = new FastStringBuilder(2048);
			glyphFixerOutput = new FastStringBuilder(2048);
		}

		public static void FixRTL(string input, FastStringBuilder output, bool farsi = true, bool fixTextTags = true, bool preserveNumbers = false)
		{
			inputBuilder.SetValue(input);
			TashkeelFixer.RemoveTashkeel(inputBuilder);
			GlyphFixer.Fix(inputBuilder, glyphFixerOutput, preserveNumbers, farsi, fixTextTags);
			TashkeelFixer.RestoreTashkeel(glyphFixerOutput);
			TashkeelFixer.FixShaddaCombinations(glyphFixerOutput);
			LigatureFixer.Fix(glyphFixerOutput, output, farsi, fixTextTags, preserveNumbers);
			inputBuilder.Clear();
		}
	}
	public struct TashkeelLocation
	{
		public char Tashkeel { get; set; }

		public int Position { get; set; }

		public TashkeelLocation(TashkeelCharacters tashkeel, int position)
		{
			this = default(TashkeelLocation);
			Tashkeel = (char)tashkeel;
			Position = position;
		}
	}
	public static class TashkeelFixer
	{
		private static readonly List<TashkeelLocation> TashkeelLocations = new List<TashkeelLocation>(100);

		private static readonly string ShaddaDammatan = new string(new char[2] { '\u0651', '\u064c' });

		private static readonly string ShaddaKasratan = new string(new char[2] { '\u0651', '\u064d' });

		private static readonly string ShaddaSuperscriptAlef = new string(new char[2] { '\u0651', '\u0670' });

		private static readonly string ShaddaFatha = new string(new char[2] { '\u0651', '\u064e' });

		private static readonly string ShaddaDamma = new string(new char[2] { '\u0651', '\u064f' });

		private static readonly string ShaddaKasra = new string(new char[2] { '\u0651', '\u0650' });

		private static readonly string ShaddaWithFathaIsolatedForm = 'ﱠ'.ToString();

		private static readonly string ShaddaWithDammaIsolatedForm = 'ﱡ'.ToString();

		private static readonly string ShaddaWithKasraIsolatedForm = 'ﱢ'.ToString();

		private static readonly string ShaddaWithDammatanIsolatedForm = 'ﱞ'.ToString();

		private static readonly string ShaddaWithKasratanIsolatedForm = 'ﱟ'.ToString();

		private static readonly string ShaddaWithSuperscriptAlefIsolatedForm = 'ﱣ'.ToString();

		private static readonly HashSet<char> TashkeelCharactersSet = new HashSet<char>
		{
			'\u064b', '\u064c', '\u064d', '\u064e', '\u064f', '\u0650', '\u0651', '\u0652', '\u0653', '\u0670',
			'ﱞ', 'ﱟ', 'ﱠ', 'ﱡ', 'ﱢ', 'ﱣ'
		};

		private static readonly Dictionary<char, char> ShaddaCombinationMap = new Dictionary<char, char>
		{
			['\u064c'] = 'ﱞ',
			['\u064d'] = 'ﱟ',
			['\u064e'] = 'ﱠ',
			['\u064f'] = 'ﱡ',
			['\u0650'] = 'ﱢ',
			['\u0670'] = 'ﱣ'
		};

		public static void RemoveTashkeel(FastStringBuilder input)
		{
			TashkeelLocations.Clear();
			int num = 0;
			for (int i = 0; i < input.Length; i++)
			{
				int num2 = input.Get(i);
				if (Char32Utils.IsUnicode16Char(num2) && TashkeelCharactersSet.Contains((char)num2))
				{
					TashkeelLocations.Add(new TashkeelLocation((TashkeelCharacters)num2, i));
					continue;
				}
				input.Set(num, num2);
				num++;
			}
			input.Length = num;
		}

		public static void RestoreTashkeel(FastStringBuilder letters)
		{
			foreach (TashkeelLocation tashkeelLocation in TashkeelLocations)
			{
				letters.Insert(tashkeelLocation.Position, tashkeelLocation.Tashkeel);
			}
		}

		public static void FixShaddaCombinations(FastStringBuilder input)
		{
			int num = 0;
			int num2 = 0;
			while (num2 < input.Length)
			{
				int num3 = input.Get(num2);
				int num4 = ((num2 < input.Length - 1) ? input.Get(num2 + 1) : 0);
				if (num3 == 1617 && ShaddaCombinationMap.ContainsKey((char)num4))
				{
					input.Set(num, ShaddaCombinationMap[(char)num4]);
					num++;
					num2 += 2;
				}
				else
				{
					input.Set(num, num3);
					num++;
					num2++;
				}
			}
			input.Length = num;
		}
	}
	public static class TextUtils
	{
		private const char LowerCaseA = 'a';

		private const char UpperCaseA = 'A';

		private const char LowerCaseZ = 'z';

		private const char UpperCaseZ = 'Z';

		private const char HebrewLow = '\u0591';

		private const char HebrewHigh = '״';

		private const char ArabicBaseBlockLow = '\u0600';

		private const char ArabicBaseBlockHigh = 'ۿ';

		private const char ArabicExtendedABlockLow = 'ࢠ';

		private const char ArabicExtendedABlockHigh = '\u08ff';

		private const char ArabicExtendedBBlockLow = '\u0870';

		private const char ArabicExtendedBBlockHigh = '\u089f';

		private const char ArabicPresentationFormsABlockLow = 'ﭐ';

		private const char ArabicPresentationFormsABlockHigh = '\ufdff';

		private const char ArabicPresentationFormsBBlockLow = 'ﹰ';

		private const char ArabicPresentationFormsBBlockHigh = '\ufeff';

		public static bool IsPunctuation(char ch)
		{
			throw new NotImplementedException();
		}

		public static bool IsNumber(char ch, bool preserveNumbers, bool farsi)
		{
			if (preserveNumbers)
			{
				return IsEnglishNumber(ch);
			}
			if (farsi)
			{
				return IsFarsiNumber(ch);
			}
			return IsHinduNumber(ch);
		}

		public static bool IsEnglishNumber(char ch)
		{
			return ch >= '0' && ch <= '9';
		}

		public static bool IsFarsiNumber(char ch)
		{
			return ch >= '۰' && ch <= '۹';
		}

		public static bool IsHinduNumber(char ch)
		{
			return ch >= '٠' && ch <= '٩';
		}

		public static bool IsEnglishLetter(char ch)
		{
			return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
		}

		public static bool IsHebrewCharacter(char ch)
		{
			return ch >= '\u0591' && ch <= '״';
		}

		public static bool IsArabicCharacter(char ch)
		{
			return (ch >= '\u0600' && ch <= 'ۿ') || (ch >= 'ࢠ' && ch <= '\u08ff') || (ch >= '\u0870' && ch <= '\u089f') || (ch >= 'ﭐ' && ch <= '\ufdff') || (ch >= 'ﹰ' && ch <= '\ufeff');
		}

		public static bool IsRTLCharacter(char ch)
		{
			if (IsHebrewCharacter(ch))
			{
				return true;
			}
			if (IsArabicCharacter(ch))
			{
				return true;
			}
			return false;
		}

		public static bool IsGlyphFixedArabicCharacter(char ch)
		{
			if (ch >= 'ﺀ' && ch <= 'ﺃ')
			{
				return true;
			}
			if (ch >= 'ﺍ' && ch <= 'ﺐ')
			{
				return true;
			}
			if (ch >= 'ﺃ' && ch <= 'ﺆ')
			{
				return true;
			}
			if (ch >= 'ﺅ' && ch <= 'ﺈ')
			{
				return true;
			}
			if (ch >= 'ﺇ' && ch <= 'ﺊ')
			{
				return true;
			}
			if (ch >= 'ﻯ' && ch <= 'ﻲ')
			{
				return true;
			}
			if (ch >= 'ﺉ' && ch <= 'ﺌ')
			{
				return true;
			}
			if (ch >= 'ﺏ' && ch <= 'ﺒ')
			{
				return true;
			}
			if (ch >= 'ﺕ' && ch <= 'ﺘ')
			{
				return true;
			}
			if (ch >= 'ﺙ' && ch <= 'ﺜ')
			{
				return true;
			}
			if (ch >= 'ﺝ' && ch <= 'ﺠ')
			{
				return true;
			}
			if (ch >= 'ﺡ' && ch <= 'ﺤ')
			{
				return true;
			}
			if (ch >= 'ﺥ' && ch <= 'ﺨ')
			{
				return true;
			}
			if (ch >= 'ﺩ' && ch <= 'ﺬ')
			{
				return true;
			}
			if (ch >= 'ﺫ' && ch <= 'ﺮ')
			{
				return true;
			}
			if (ch >= 'ﺭ' && ch <= 'ﺰ')
			{
				return true;
			}
			if (ch >= 'ﺯ' && ch <= 'ﺲ')
			{
				return true;
			}
			if (ch >= 'ﺱ' && ch <= 'ﺴ')
			{
				return true;
			}
			if (ch >= 'ﺵ' && ch <= 'ﺸ')
			{
				return true;
			}
			if (ch >= 'ﺹ' && ch <= 'ﺼ')
			{
				return true;
			}
			if (ch >= 'ﺽ' && ch <= 'ﻀ')
			{
				return true;
			}
			if (ch >= 'ﻁ' && ch <= 'ﻄ')
			{
				return true;
			}
			if (ch >= 'ﻅ' && ch <= 'ﻈ')
			{
				return true;
			}
			if (ch >= 'ﻉ' && ch <= 'ﻌ')
			{
				return true;
			}
			if (ch >= 'ﻍ' && ch <= 'ﻐ')
			{
				return true;
			}
			if (ch >= 'ﻑ' && ch <= 'ﻔ')
			{
				return true;
			}
			if (ch >= 'ﻕ' && ch <= 'ﻘ')
			{
				return true;
			}
			if (ch >= 'ﻙ' && ch <= 'ﻜ')
			{
				return true;
			}
			if (ch >= 'ﻝ' && ch <= 'ﻠ')
			{
				return true;
			}
			if (ch >= 'ﻡ' && ch <= 'ﻤ')
			{
				return true;
			}
			if (ch >= 'ﻥ' && ch <= 'ﻨ')
			{
				return true;
			}
			if (ch >= 'ﻩ' && ch <= 'ﻬ')
			{
				return true;
			}
			if (ch >= 'ﻭ' && ch <= 'ﻰ')
			{
				return true;
			}
			if (ch >= 'ﻱ' && ch <= 'ﻴ')
			{
				return true;
			}
			if (ch >= 'ﺁ' && ch <= 'ﺄ')
			{
				return true;
			}
			if (ch >= 'ﺓ' && ch <= 'ﺖ')
			{
				return true;
			}
			if (ch >= 'ﭖ' && ch <= 'ﭙ')
			{
				return true;
			}
			if (ch >= 'ﯼ' && ch <= 'ﯿ')
			{
				return true;
			}
			if (ch >= 'ﭺ' && ch <= 'ﭽ')
			{
				return true;
			}
			if (ch >= 'ﮊ' && ch <= 'ﮍ')
			{
				return true;
			}
			if (ch >= 'ﮒ' && ch <= 'ﮕ')
			{
				return true;
			}
			if (ch >= 'ﮎ' && ch <= 'ﮑ')
			{
				return true;
			}
			switch (ch)
			{
			case 'ﻳ':
				return true;
			case 'ﻵ':
				return true;
			case 'ﻷ':
				return true;
			case 'ﻹ':
				return true;
			default:
				switch (ch)
				{
				case 'ء':
				case 'آ':
				case 'أ':
				case 'ؤ':
				case 'إ':
				case 'ئ':
				case 'ا':
				case 'ب':
				case 'ة':
				case 'ت':
				case 'ث':
				case 'ج':
				case 'ح':
				case 'خ':
				case 'د':
				case 'ذ':
				case 'ر':
				case 'ز':
				case 'س':
				case 'ش':
				case 'ص':
				case 'ض':
				case 'ط':
				case 'ظ':
				case 'ع':
				case 'غ':
				case 'ـ':
				case 'ف':
				case 'ق':
				case 'ك':
				case 'ل':
				case 'م':
				case 'ن':
				case 'ه':
				case 'و':
				case 'ى':
				case 'ي':
				case 'پ':
				case 'چ':
				case 'ژ':
				case 'ک':
				case 'گ':
				case 'ی':
				case '\u200c':
					return true;
				default:
					return false;
				}
			}
		}

		public static bool IsRTLInput(string input)
		{
			bool flag = false;
			foreach (char c in input)
			{
				switch (c)
				{
				case '<':
					flag = true;
					continue;
				case '>':
					flag = false;
					continue;
				}
				if (flag || !char.IsLetter(c))
				{
					continue;
				}
				return IsRTLCharacter(c);
			}
			return false;
		}
	}
}
namespace PastimeReading
{
	public static class CustomArmsAnimation
	{
		private static Animator currentAnimator;

		private static Animator animatorPreset;

		private static Animator vanillaAnimator;

		private static GameObject animatorHolder;

		private static GameObject objectToAppendTo;

		private static GameObject propPointRight;

		private static GameObject propPointLeft;

		private static GameObject toolRight;

		private static GameObject toolLeft;

		public static void Register(AssetBundle assBun, string mainBundleObject)
		{
			animatorHolder = assBun.LoadAsset<GameObject>(mainBundleObject);
			if (Object.op_Implicit((Object)(object)animatorHolder))
			{
				animatorHolder = Object.Instantiate<GameObject>(animatorHolder);
				Object.DontDestroyOnLoad((Object)(object)animatorHolder);
				animatorHolder.active = false;
				GameObject topLevelCharacterFpsPlayer = GameManager.GetTopLevelCharacterFpsPlayer();
				object obj;
				if (topLevelCharacterFpsPlayer == null)
				{
					obj = null;
				}
				else
				{
					Transform obj2 = topLevelCharacterFpsPlayer.transform.Find("NEW_FPHand_Rig/GAME_DATA");
					obj = ((obj2 != null) ? ((Component)obj2).gameObject : null);
				}
				objectToAppendTo = (GameObject)obj;
				Transform obj3 = objectToAppendTo.transform.FindChild("Origin/HipJoint/Chest_Joint/Camera_Weapon_Offset/Shoulder_Joint/Shoulder_Joint_Offset/Right_Shoulder_Joint_Offset/RightClavJoint/RightShoulderJoint/RightElbowJoint/RightWristJoint/RightPalm/right_prop_point");
				propPointRight = ((obj3 != null) ? ((Component)obj3).gameObject : null);
				Transform obj4 = objectToAppendTo.transform.FindChild("Origin/HipJoint/Chest_Joint/Camera_Weapon_Offset/Shoulder_Joint/Shoulder_Joint_Offset/Left_Shoulder_Joint_Offset/LeftClavJoint/LeftShoulderJoint/LeftElbowJoint/LeftWristJoint/LeftPalm/left_prop_point");
				propPointLeft = ((obj4 != null) ? ((Component)obj4).gameObject : null);
				animatorPreset = animatorHolder.GetComponent<Animator>();
				vanillaAnimator = ((Component)objectToAppendTo.transform.GetParent()).GetComponent<Animator>();
			}
		}

		public static void AppendTool(AssetBundle assBun, string objectToSearch, bool rightHand)
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = assBun.LoadAsset<GameObject>(objectToSearch);
			if (Object.op_Implicit((Object)(object)val))
			{
				val = Object.Instantiate<GameObject>(val);
				val.layer = LayerMask.NameToLayer("Weapon");
				((Renderer)val.GetComponent<MeshRenderer>()).material.shader = Shader.Find("Shader Forge/TLD_StandardSkinned");
				val.transform.SetParent(rightHand ? propPointRight.transform : propPointLeft.transform);
				val.transform.localPosition = Vector3.zero;
				val.transform.localEulerAngles = Vector3.zero;
				val.active = false;
				if (rightHand)
				{
					toolRight = val;
				}
				else
				{
					toolLeft = val;
				}
			}
		}

		public static void Activate()
		{
			if (!Object.op_Implicit((Object)(object)objectToAppendTo))
			{
				return;
			}
			Animator component = objectToAppendTo.GetComponent<Animator>();
			if (!Object.op_Implicit((Object)(object)currentAnimator) || (Object.op_Implicit((Object)(object)component) && (Object)(object)currentAnimator != (Object)(object)component))
			{
				if (Object.op_Implicit((Object)(object)component))
				{
					Object.Destroy((Object)(object)component);
				}
				currentAnimator = objectToAppendTo.AddComponent<Animator>();
				((Behaviour)currentAnimator).enabled = false;
				currentAnimator.runtimeAnimatorController = animatorPreset.runtimeAnimatorController;
				currentAnimator.avatar = animatorPreset.avatar;
			}
			((Behaviour)vanillaAnimator).enabled = false;
			((Behaviour)currentAnimator).enabled = true;
		}

		public static void Done()
		{
			((Behaviour)vanillaAnimator).enabled = true;
			((Behaviour)currentAnimator).enabled = false;
		}
	}
	public static class PageManager
	{
		public static string currentTurn = null;

		public static int currentPage;

		public static float currentFontSize;

		public static int currentBookTexture = Settings.options.bookTexture;

		public static int currentAlignment = Settings.options.textAlignment;

		public static string bookFileName = "book.txt";

		private static float a = 0.5f;

		private static float f = 0f;

		private static int i = 0;

		private static float o;

		private static int animatorSifter = 0;

		public static string bookTitle;

		public static string bookAuthor;

		public static string bookContents;

		public static string[] splitPages;

		private static int firstChar;

		private static int lastChar;

		private static readonly int chunkSize = 20000;

		private static int splitCurrentSymbol = 0;

		private static string chunkContents;

		private static FastStringBuilder pageContentRTL = new FastStringBuilder(2048);

		public static bool currentlyRTL = Settings.options.enableRTL;

		public static Color bookTitleColor = Color.white;

		public static Color bookAuthorColor = Color.white;

		private static readonly string emptyBookPlaceholder = "All work and no play makes Waltz a dull boy.\nAll work and no play makes Waltz a dull boy.\n All work and no play mmakes Waltz a dull boy.\nv All work and no PLay ma es Waltz a dull boy.\nAll work and no play makes Waltz a dull boy.\nAll work and no play makes Waltz a dull boy.\nAll work and no ply maKes Waltz a dull boy.\nAll work and no pllay makes Waltz a dull boy.\nAll work and no play makes Walt z dyll boy.\nAll work and no play makes Waltz a dull boy.\nAll work and no play makes Waltz a dull boy.\nAll work and no play makes Waltz a dullboy.\nAll work and no play makes Waltz a dull boy.\nAll work and NO play makes WALz dull boy.\nall work and no play makes Waltz a dull boy.\nAll work and no plaay makes Waltz a dull boy.\nAll work and no play makes Waltz a dull bog.\nA111 work and no play makes Waltz a dull bot.\nAll work and noplay makes Watz a dull boy.\nAll work and no play makes Waltz a dull boy.\nAll work and no play makes Waltz a dull boy.\nAll work and no ply maKes Waltz a dyll boy.\nAll work and no play makes Waltz a dull boy.";

		public static void InitPages(string stage)
		{
			//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0798: Unknown result type (might be due to invalid IL or missing references)
			//IL_0718: Unknown result type (might be due to invalid IL or missing references)
			//IL_047b: Unknown result type (might be due to invalid IL or missing references)
			//IL_049b: Unknown result type (might be due to invalid IL or missing references)
			splitCurrentSymbol = 0;
			if (stage == "rtl")
			{
				currentlyRTL = Settings.options.enableRTL;
				if (currentlyRTL)
				{
					ReadMain.p1Text.isRightToLeftText = true;
					ReadMain.p2Text.isRightToLeftText = true;
					ReadMain.h1Text.isRightToLeftText = true;
					ReadMain.h2Text.isRightToLeftText = true;
					ReadMain.titleText.isRightToLeftText = true;
					ReadMain.authorText.isRightToLeftText = true;
				}
				else
				{
					ReadMain.p1Text.isRightToLeftText = false;
					ReadMain.p2Text.isRightToLeftText = false;
					ReadMain.h1Text.isRightToLeftText = false;
					ReadMain.h2Text.isRightToLeftText = false;
					ReadMain.titleText.isRightToLeftText = false;
					ReadMain.authorText.isRightToLeftText = false;
				}
			}
			if (stage == "read")
			{
				StreamReader streamReader = new StreamReader(ReadMain.modsPath + "/pastimeReading/" + bookFileName);
				while (!streamReader.EndOfStream)
				{
					for (int i = 0; i < 2; i++)
					{
						if (i == 0)
						{
							bookTitle = streamReader.ReadLine();
						}
						if (i == 1)
						{
							bookAuthor = streamReader.ReadLine();
						}
					}
					bookContents = streamReader.ReadToEnd();
					if (currentlyRTL)
					{
						bookContents = RTLconvert(bookContents);
					}
				}
				if (bookContents == null || bookContents.Length <= 1)
				{
					MelonLogger.Msg(ConsoleColor.Yellow, "书籍内容为空!");
					bookContents = emptyBookPlaceholder;
				}
			}
			if (stage == "font")
			{
				ReadMain.p1Text.fontSize = Settings.options.fontSize;
				ReadMain.p2Text.fontSize = Settings.options.fontSize;
				ReadMain.h1Text.fontSize = Settings.options.fontSize;
				ReadMain.h2Text.fontSize = Settings.options.fontSize;
				currentFontSize = Settings.options.fontSize;
				switch (Settings.options.textAlignment)
				{
				case 0:
					ReadMain.p1Text.alignment = (TextAlignmentOptions)264;
					ReadMain.p2Text.alignment = (TextAlignmentOptions)264;
					ReadMain.h1Text.alignment = (TextAlignmentOptions)264;
					ReadMain.h2Text.alignment = (TextAlignmentOptions)264;
					break;
				case 1:
					ReadMain.p1Text.alignment = (TextAlignmentOptions)257;
					ReadMain.p2Text.alignment = (TextAlignmentOptions)257;
					ReadMain.h1Text.alignment = (TextAlignmentOptions)257;
					ReadMain.h2Text.alignment = (TextAlignmentOptions)257;
					break;
				case 2:
					ReadMain.p1Text.alignment = (TextAlignmentOptions)260;
					ReadMain.p2Text.alignment = (TextAlignmentOptions)260;
					ReadMain.h1Text.alignment = (TextAlignmentOptions)260;
					ReadMain.h2Text.alignment = (TextAlignmentOptions)260;
					break;
				case 3:
					ReadMain.p1Text.alignment = (TextAlignmentOptions)258;
					ReadMain.p2Text.alignment = (TextAlignmentOptions)258;
					ReadMain.h1Text.alignment = (TextAlignmentOptions)258;
					ReadMain.h2Text.alignment = (TextAlignmentOptions)258;
					break;
				}
				currentAlignment = Settings.options.textAlignment;
			}
			if (stage == "split")
			{
				splitPages = new string[0];
				while (bookContents.Length > splitCurrentSymbol)
				{
					bool flag = false;
					if (bookContents.Length < splitCurrentSymbol + chunkSize)
					{
						flag = true;
						chunkContents = bookContents.Substring(splitCurrentSymbol);
					}
					else
					{
						chunkContents = bookContents.Substring(splitCurrentSymbol, chunkSize);
					}
					ReadMain.p1Text.overflowMode = (TextOverflowModes)5;
					ReadMain.p1Text.m_text = chunkContents;
					Utility.Log(ConsoleColor.Gray, "ForceMeshUpdate - Start");
					ReadMain.p1Text.ForceMeshUpdate(true, false);
					Utility.Log(ConsoleColor.Gray, "ForceMeshUpdate - End, #Pages: " + splitPages.Length);
					string[] array = new string[ReadMain.p1Text.textInfo.pageCount];
					for (int j = 0; j < ReadMain.p1Text.textInfo.pageCount; j++)
					{
						firstChar = ((Il2CppArrayBase<TMP_PageInfo>)(object)ReadMain.p1Text.textInfo.pageInfo)[j].firstCharacterIndex;
						lastChar = ((Il2CppArrayBase<TMP_PageInfo>)(object)ReadMain.p1Text.textInfo.pageInfo)[j].lastCharacterIndex;
						if (lastChar > firstChar)
						{
							array[j] = chunkContents.Substring(firstChar, lastChar - firstChar + 1);
						}
						else
						{
							array[j] = chunkContents.Substring(firstChar);
						}
					}
					if (!flag)
					{
						splitCurrentSymbol = splitCurrentSymbol + chunkSize - array[^1].Length;
						Array.Resize(ref array, array.Length - 1);
					}
					else
					{
						splitCurrentSymbol = splitCurrentSymbol + chunkSize + 1;
					}
					int num = splitPages.Length;
					Array.Resize(ref splitPages, num + array.Length);
					array.CopyTo(splitPages, num);
				}
			}
			if (stage == "page")
			{
				if (Settings.options.currentPage < splitPages.Length)
				{
					currentPage = Settings.options.currentPage;
				}
				else if (splitPages.Length % 2 != 0)
				{
					currentPage = splitPages.Length;
				}
				else
				{
					currentPage = splitPages.Length - 1;
				}
			}
			if (!(stage == "setup"))
			{
				return;
			}
			TurnpageVisible(state: false);
			ReadMain.p1Text.overflowMode = (TextOverflowModes)0;
			ReadMain.p2Text.overflowMode = (TextOverflowModes)0;
			ReadMain.h1Text.overflowMode = (TextOverflowModes)0;
			ReadMain.h2Text.overflowMode = (TextOverflowModes)0;
			if (!currentlyRTL)
			{
				ReadMain.titleText.text = bookTitle;
				ReadMain.authorText.text = bookAuthor;
			}
			else
			{
				ReadMain.titleText.text = RTLconvert(bookTitle);
				ReadMain.authorText.text = RTLconvert(bookAuthor);
			}
			((Graphic)ReadMain.titleText).color = bookTitleColor;
			((Graphic)ReadMain.authorText).color = bookAuthorColor;
			if (Settings.options.enableRTL)
			{
				ReadMain.hands.transform.localScale = new Vector3(-1f, 1f, 1f);
				SetPage("p2", currentPage);
				if (splitPages.Length > 1)
				{
					SetPage("p1", currentPage + 1);
					return;
				}
				ReadMain.p1Text.text = "";
				ReadMain.p1PageText.text = "";
			}
			else
			{
				ReadMain.hands.transform.localScale = new Vector3(1f, 1f, 1f);
				SetPage("p1", currentPage);
				if (splitPages.Length > 1)
				{
					SetPage("p2", currentPage + 1);
					return;
				}
				ReadMain.p2Text.text = "";
				ReadMain.p2PageText.text = "";
			}
		}

		public static string RTLconvert(string text)
		{
			pageContentRTL.Clear();
			RTLSupport.FixRTL(text, pageContentRTL, farsi: false, fixTextTags: true, preserveNumbers: true);
			pageContentRTL.Reverse();
			return pageContentRTL.ToString();
		}

		public static void SetPage(string page, int num)
		{
			if (page == "p1")
			{
				if (num <= splitPages.Length)
				{
					ReadMain.p1Text.text = splitPages[num - 1];
				}
				else
				{
					ReadMain.p1Text.text = null;
				}
				ReadMain.p1PageText.text = num.ToString();
			}
			if (page == "p2")
			{
				if (num <= splitPages.Length)
				{
					ReadMain.p2Text.text = splitPages[num - 1];
				}
				else
				{
					ReadMain.p2Text.text = null;
				}
				ReadMain.p2PageText.text = num.ToString();
			}
			if (page == "h1")
			{
				if (num <= splitPages.Length)
				{
					ReadMain.h1Text.text = splitPages[num - 1];
				}
				else
				{
					ReadMain.h1Text.text = null;
				}
				ReadMain.h1PageText.text = num.ToString();
			}
			if (page == "h2")
			{
				if (num <= splitPages.Length)
				{
					ReadMain.h2Text.text = splitPages[num - 1];
				}
				else
				{
					ReadMain.h2Text.text = null;
				}
				ReadMain.h2PageText.text = num.ToString();
			}
		}

		public static void IdleFluc()
		{
			o += Time.deltaTime;
			if (o <= 0.1f)
			{
				return;
			}
			o = 0f;
			if (f < 1f)
			{
				a += ((i == 0) ? 0.03f : (-0.03f));
				if (a > 0.9f)
				{
					i = 1;
				}
				if (a < 0.1f)
				{
					i = 0;
				}
				ReadMain.handsAnim.SetFloat("idle_random", a);
				f += 0.1f;
			}
			else
			{
				i = Random.Range(0, 2);
				f = 0f;
				if ((double)Random.value <= 0.001)
				{
					ReadMain.handsAnim.SetTrigger("scratch_ear");
				}
			}
		}

		public static void AnimatorStateSifter(string state, float clipTimeStart, float clipTimeEnd)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			AnimatorStateInfo currentAnimatorStateInfo = ReadMain.handsAnim.GetCurrentAnimatorStateInfo(0);
			float num = ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).normalizedTime % 1f;
			currentAnimatorStateInfo = ReadMain.handsAnim.GetCurrentAnimatorStateInfo(0);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName(state))
			{
				if (animatorSifter == 0)
				{
					animatorSifter = 1;
					PageFlip("firstFrame", Settings.options.enableRTL);
				}
				if (num > clipTimeStart && animatorSifter < 2)
				{
					animatorSifter = 2;
					PageFlip("nearFirstFrame", Settings.options.enableRTL);
				}
				if (num > clipTimeEnd && animatorSifter < 3)
				{
					animatorSifter = 3;
					PageFlip("nearLastFrame", Settings.options.enableRTL);
				}
			}
			else if (animatorSifter != 0)
			{
				animatorSifter = 0;
				PageFlip("lastFrame", Settings.options.enableRTL);
				currentTurn = null;
			}
		}

		private static void TurnpageVisible(bool state)
		{
			ReadMain.turnpage.SetActive(state);
			ReadMain.h1.SetActive(state);
			ReadMain.h2.SetActive(state);
		}

		public static void PageFlip(string aEvent, bool RTL)
		{
			string page = "p1";
			string page2 = "p2";
			string page3 = "h1";
			string page4 = "h2";
			if (RTL)
			{
				page = "p2";
				page2 = "p1";
				page3 = "h2";
				page4 = "h1";
			}
			if (aEvent == "firstFrame")
			{
				TurnpageVisible(state: true);
				if (currentTurn == "next")
				{
					ReadMain.p2.SetActive(false);
					SetPage(page3, currentPage + 2);
					SetPage(page4, currentPage + 1);
					SetPage(page2, currentPage + 3);
				}
				if (currentTurn == "prev")
				{
					ReadMain.p1.SetActive(false);
					SetPage(page3, currentPage);
					SetPage(page4, currentPage - 1);
					SetPage(page, currentPage - 2);
				}
			}
			if (aEvent == "nearFirstFrame")
			{
				if (currentTurn == "next")
				{
					ReadMain.p2.SetActive(true);
				}
				if (currentTurn == "prev")
				{
					ReadMain.p1.SetActive(true);
				}
			}
			if (aEvent == "nearLastFrame")
			{
				if (currentTurn == "next")
				{
					ReadMain.p1.SetActive(false);
				}
				if (currentTurn == "prev")
				{
					ReadMain.p2.SetActive(false);
				}
			}
			if (aEvent == "lastFrame")
			{
				TurnpageVisible(state: false);
				if (currentTurn == "next")
				{
					ReadMain.p1.SetActive(true);
					SetPage(page, currentPage + 2);
					currentPage += 2;
				}
				if (currentTurn == "prev")
				{
					ReadMain.p2.SetActive(true);
					SetPage(page2, currentPage - 1);
					currentPage -= 2;
				}
				Settings.options.currentPage = currentPage;
				((JsonModSettings)Settings.options).Save();
			}
		}
	}
	public static class ReadInstance
	{
		private static readonly int renderLayer = LayerMask.NameToLayer("Weapon");

		public static readonly Shader vanillaSkinShader = Shader.Find("Shader Forge/TLD_StandardSkinned");

		public static readonly Shader vanillaDefaultShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

		public static void ReadInstanceLoad()
		{
			ReadMain.handsAsset = ReadMain.loadBundle.LoadAsset<GameObject>("hands_with_book");
			ReadMain.pCamAsset = ReadMain.loadBundle.LoadAsset<GameObject>("p1-2Cam");
			ReadMain.hCamAsset = ReadMain.loadBundle.LoadAsset<GameObject>("h1-2Cam");
			ReadMain.cCamAsset = ReadMain.loadBundle.LoadAsset<GameObject>("coverCam");
			ReadMain.hands = Object.Instantiate<GameObject>(ReadMain.handsAsset);
			ReadMain.pCam = Object.Instantiate<GameObject>(ReadMain.pCamAsset);
			ReadMain.hCam = Object.Instantiate<GameObject>(ReadMain.hCamAsset);
			ReadMain.cCam = Object.Instantiate<GameObject>(ReadMain.cCamAsset);
			ReadMain.handsFMesh = ((Component)ReadMain.hands.transform.Find("readingArmsF")).gameObject;
			ReadMain.handsMMesh = ((Component)ReadMain.hands.transform.Find("readingArmsM")).gameObject;
			Utility.Log(ConsoleColor.Gray, "ReadInstanceLoad - Instantiated");
			ReadMain.weaponCamera = GameObject.Find("/CHARACTER_FPSPlayer/WeaponView/WeaponCamera");
			Utility.Log(ConsoleColor.Gray, "ReadInstanceLoad - Found camera");
			UpdateBookTexture();
			Utility.Log(ConsoleColor.Gray, "ReadInstanceLoad - Book texture update");
			ReadMain.handsAnim = ReadMain.hands.GetComponent<Animator>();
			Utility.Log(ConsoleColor.Gray, "ReadInstanceLoad - Found animator");
			foreach (Transform componentsInChild in ReadMain.hands.GetComponentsInChildren<Transform>())
			{
				((Component)componentsInChild).gameObject.layer = renderLayer;
			}
			Utility.Log(ConsoleColor.Gray, "ReadInstanceLoad - Layer set");
			ReadMain.hands.transform.SetParent(((Object)(object)ReadMain.weaponCamera != (Object)null) ? ReadMain.weaponCamera.transform : null);
			ReadMain.cCam.transform.SetParent(ReadMain.hands.transform);
			ReadMain.pCam.transform.SetParent(ReadMain.hands.transform);
			ReadMain.hCam.transform.SetParent(ReadMain.hands.transform);
			Utility.Log(ConsoleColor.Gray, "ReadInstanceLoad - Parented");
			ReadMain.turnpage = ((Component)ReadMain.hands.transform.Find("readingBook_turnpage")).gameObject;
			ReadMain.p1 = ((Component)ReadMain.hands.transform.Find("readingBook_textField_p1")).gameObject;
			ReadMain.p2 = ((Component)ReadMain.hands.transform.Find("readingBook_textField_p2")).gameObject;
			ReadMain.h1 = ((Component)ReadMain.hands.transform.Find("readingBook_textField_h1")).gameObject;
			ReadMain.h2 = ((Component)ReadMain.hands.transform.Find("readingBook_textField_h2")).gameObject;
			ReadMain.titleText = ((Component)ReadMain.cCam.transform.GetChild(0)).GetComponent<TMP_Text>();
			ReadMain.authorText = ((Component)ReadMain.cCam.transform.GetChild(1)).GetComponent<TMP_Text>();
			ReadMain.p1Text = ((Component)ReadMain.pCam.transform.GetChild(0)).GetComponent<TMP_Text>();
			ReadMain.p2Text = ((Component)ReadMain.pCam.transform.GetChild(1)).GetComponent<TMP_Text>();
			ReadMain.p1PageText = ((Component)ReadMain.pCam.transform.GetChild(2)).GetComponent<TMP_Text>();
			ReadMain.p2PageText = ((Component)ReadMain.pCam.transform.GetChild(3)).GetComponent<TMP_Text>();
			ReadMain.h1Text = ((Component)ReadMain.hCam.transform.GetChild(0)).GetComponent<TMP_Text>();
			ReadMain.h2Text = ((Component)ReadMain.hCam.transform.GetChild(1)).GetComponent<TMP_Text>();
			ReadMain.h1PageText = ((Component)ReadMain.hCam.transform.GetChild(2)).GetComponent<TMP_Text>();
			ReadMain.h2PageText = ((Component)ReadMain.hCam.transform.GetChild(3)).GetComponent<TMP_Text>();
			Utility.Log(ConsoleColor.Gray, "ReadInstanceLoad - Book loaded");
		}

		public static void UpdateBookTexture()
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			string path = null;
			switch (Settings.options.bookTexture)
			{
			case 0:
				path = "Mods/pastimeReading/textures/simpleRed.png";
				break;
			case 1:
				path = "Mods/pastimeReading/textures/simpleBlue.png";
				break;
			case 2:
				path = "Mods/pastimeReading/textures/simpleWhite.png";
				break;
			case 3:
				path = "Mods/pastimeReading/textures/simpleYellow.png";
				break;
			case 4:
				path = "Mods/pastimeReading/textures/detailGray.png";
				break;
			case 5:
				path = "Mods/pastimeReading/textures/detailYellow.png";
				break;
			case 6:
				path = "Mods/pastimeReading/textures/detailBlack.png";
				break;
			}
			if (File.Exists(path))
			{
				Texture2D val = new Texture2D(2, 2);
				GameObject gameObject = ((Component)ReadMain.hands.transform.Find("readingBook")).gameObject;
				ImageConversion.LoadImage(val, Il2CppStructArray<byte>.op_Implicit(File.ReadAllBytes(path)));
				((Renderer)gameObject.GetComponent<SkinnedMeshRenderer>()).sharedMaterial.mainTexture = (Texture)(object)val;
				((Renderer)gameObject.GetComponent<SkinnedMeshRenderer>()).sharedMaterial.shader = vanillaSkinShader;
				PageManager.bookTitleColor = val.GetPixel(1, 0);
				PageManager.bookAuthorColor = val.GetPixel(5, 0);
				PageManager.currentBookTexture = Settings.options.bookTexture;
			}
			else
			{
				MelonLogger.Msg(ConsoleColor.Red, "找不到书本贴图");
				Settings.options.bookTexture = PageManager.currentBookTexture;
			}
		}
	}
	public class ReadMain : MelonMod
	{
		public static AssetBundle loadBundle;

		public static AssetBundle loadBundle2;

		public static GameObject handsAsset;

		public static GameObject hands = null;

		public static Animator handsAnim;

		public static GameObject vanillaHandsF = null;

		public static GameObject vanillaHandsM = null;

		public static GameObject handsFMesh;

		public static GameObject handsMMesh;

		private static bool loadVanillaHands = false;

		public static GameObject pCamAsset;

		public static GameObject hCamAsset;

		public static GameObject cCamAsset;

		public static GameObject pCam = null;

		public static GameObject hCam = null;

		public static GameObject cCam = null;

		public static GameObject turnpage;

		public static GameObject p1;

		public static GameObject p2;

		public static GameObject h1;

		public static GameObject h2;

		public static TMP_Text titleText;

		public static TMP_Text authorText;

		public static TMP_Text p1Text;

		public static TMP_Text p2Text;

		public static TMP_Text h1Text;

		public static TMP_Text h2Text;

		public static TMP_Text p1PageText;

		public static TMP_Text p2PageText;

		public static TMP_Text h1PageText;

		public static TMP_Text h2PageText;

		public static GameObject weaponCamera;

		public static string modsPath;

		public static bool usingBook = false;

		public static bool bookIsOpened = false;

		private static bool bookClosing = false;

		private static string currentState = "pocket";

		private static string currentCharacter;

		private static float slowdownLerp;

		private static bool interrupted;

		private static bool timescaleReset;

		public static bool lockInteraction;

		public static bool gameStarted;

		public static readonly float iteractionAllowanceAngle = 27f;

		public override void OnInitializeMelon()
		{
			modsPath = Path.GetFullPath(typeof(MelonMod).Assembly.Location + "\\..\\..\\..\\Mods");
			loadBundle = AssetBundle.LoadFromFile(modsPath + "/pastimeReading/pastimeReadingAssets.ass");
			loadBundle2 = AssetBundle.LoadFromFile(modsPath + "/pastimeReading/handtex");
			if ((Object)(object)loadBundle == (Object)null)
			{
				MelonLogger.Msg(ConsoleColor.Yellow, "加载 AssetBundle 失败");
			}
			else
			{
				Settings.OnLoad();
			}
		}

		public override void OnSceneWasInitialized(int level, string name)
		{
			if (Utility.IsScenePlayable() && (Object)(object)hands == (Object)null)
			{
				if (!SoundManager.initDone)
				{
					SoundManager.InitSounds();
					Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Sounds done");
				}
				ReadInstance.ReadInstanceLoad();
				Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Instance done");
				PageManager.InitPages("font");
				Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Font done");
				PageManager.InitPages("rtl");
				Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - RTL done");
				if (PageManager.bookContents == null || ReadSettings.settingsChanged)
				{
					PageManager.InitPages("read");
					Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Read done");
					PageManager.InitPages("split");
					Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Split done");
					PageManager.InitPages("page");
					Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Page done");
					ReadSettings.settingsChanged = false;
					Settings.options.reloadBook = false;
				}
				PageManager.InitPages("setup");
				Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Setup done");
				loadVanillaHands = true;
				Utility.Log(ConsoleColor.Gray, "OnSceneWasInitialized - Done");
				gameStarted = true;
			}
		}

		public override void OnSceneWasUnloaded(int level, string name)
		{
			currentCharacter = null;
			usingBook = false;
			bookIsOpened = false;
			currentState = "pocket";
			bookClosing = false;
			timescaleReset = true;
		}

		private static void StopOnBookClose()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			AnimatorStateInfo currentAnimatorStateInfo = handsAnim.GetCurrentAnimatorStateInfo(0);
			if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("close_book"))
			{
				currentAnimatorStateInfo = handsAnim.GetCurrentAnimatorStateInfo(0);
				if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("remove_book"))
				{
					currentAnimatorStateInfo = handsAnim.GetCurrentAnimatorStateInfo(0);
					if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("remove_book_from_opened"))
					{
						if (bookClosing)
						{
							usingBook = false;
							bookClosing = false;
						}
						return;
					}
				}
			}
			bookIsOpened = false;
			bookClosing = true;
		}

		public override void OnUpdate()
		{
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Invalid comparison between Unknown and I4
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0797: Unknown result type (might be due to invalid IL or missing references)
			//IL_079d: Invalid comparison between Unknown and I4
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ab: Invalid comparison between Unknown and I4
			//IL_0341: Unknown result type (might be due to invalid IL or missing references)
			//IL_0362: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
			if (loadVanillaHands)
			{
				((Renderer)handsFMesh.GetComponent<SkinnedMeshRenderer>()).material.shader = ReadInstance.vanillaSkinShader;
				((Renderer)handsMMesh.GetComponent<SkinnedMeshRenderer>()).material.shader = ReadInstance.vanillaSkinShader;
				((Renderer)handsFMesh.GetComponent<SkinnedMeshRenderer>()).material.mainTexture = loadBundle2.LoadAsset<Texture>("Assets/HMF_FP_Hands_Astrid.png");
				((Renderer)handsMMesh.GetComponent<SkinnedMeshRenderer>()).material.mainTexture = loadBundle2.LoadAsset<Texture>("Assets/HMM_FP_Hands_Will.png");
				loadVanillaHands = false;
			}
			if (!gameStarted)
			{
				return;
			}
			bool keyDown = InputManager.GetKeyDown(InputManager.m_CurrentContext, Settings.options.openKeyCode);
			bool flag = (Object)(object)GameManager.GetPlayerManagerComponent().m_ItemInHands != (Object)null;
			bool flag2 = string.IsNullOrEmpty(GameManager.m_ActiveScene) || (Object)(object)GameManager.GetPlayerManagerComponent() == (Object)null || GameManager.GetPlayerManagerComponent().IsInPlacementMode();
			if (keyDown && !flag2 && !InterfaceManager.IsOverlayActiveCached() && !bookClosing)
			{
				if (currentState == "title")
				{
					handsAnim.SetTrigger("open_book");
					currentState = "open";
					bookIsOpened = true;
					interrupted = false;
				}
				if (currentState == "pocket")
				{
					usingBook = true;
					handsAnim.SetTrigger("bring_book");
					currentState = "title";
					weaponCamera.GetComponent<Camera>().fieldOfView = 37.5f;
				}
			}
			if (usingBook)
			{
				if ((int)PlayerManager.m_VoicePersona == 1 && currentCharacter != "Astrid")
				{
					handsMMesh.SetActive(false);
					handsFMesh.SetActive(true);
					currentCharacter = "Astrid";
				}
				else if ((int)PlayerManager.m_VoicePersona == 0 && currentCharacter != "Will")
				{
					handsFMesh.SetActive(false);
					handsMMesh.SetActive(true);
					currentCharacter = "Will";
				}
				Quaternion rotation = weaponCamera.transform.rotation;
				float num = ((Quaternion)(ref rotation)).eulerAngles.x;
				if (num > 90f)
				{
					num = 0f;
				}
				if (num < iteractionAllowanceAngle)
				{
					lockInteraction = false;
				}
				else if (bookIsOpened)
				{
					lockInteraction = true;
				}
				float num2 = (10f / Mathf.Pow(1f + num / 55f, 1.7f) - 23f) / 100f;
				num = ((Settings.options.bookTilt != 0) ? (40f / Mathf.Pow(1f + num / 45f, 2f) - 40f) : (50f / Mathf.Pow(1f + (num - 8f) / 50f, 1.2f) - 50f));
				hands.transform.localRotation = Quaternion.Euler(num, 0f, 0f);
				hands.transform.localPosition = new Vector3(0f, num2, 0.05f);
				if (PageManager.currentTurn == "next")
				{
					PageManager.AnimatorStateSifter("next_page", 0.6f, 0.75f);
				}
				if (PageManager.currentTurn == "prev")
				{
					PageManager.AnimatorStateSifter("prev_page", 0.35f, 0.65f);
				}
				AnimatorStateInfo currentAnimatorStateInfo = handsAnim.GetCurrentAnimatorStateInfo(0);
				if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("book_open_idle"))
				{
					currentAnimatorStateInfo = handsAnim.GetCurrentAnimatorStateInfo(0);
					if (!((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("book_title_idle"))
					{
						goto IL_040a;
					}
				}
				PageManager.IdleFluc();
				goto IL_040a;
			}
			lockInteraction = false;
			goto IL_0606;
			IL_0606:
			if (bookIsOpened)
			{
				if (Settings.options.timeScale != 1f && Time.timeScale != Settings.options.timeScale)
				{
					if (Mathf.Approximately(1f, Time.timeScale))
					{
						slowdownLerp = 0f;
					}
					Time.timeScale = (GameManager.m_GlobalTimeScale = Mathf.Lerp(1f, Settings.options.timeScale, slowdownLerp += Time.unscaledDeltaTime / 2f));
					if ((int)handsAnim.updateMode != 2)
					{
						handsAnim.updateMode = (AnimatorUpdateMode)2;
						timescaleReset = false;
					}
				}
			}
			else
			{
				if (timescaleReset)
				{
					return;
				}
				if (!Mathf.Approximately(1f, Time.timeScale))
				{
					if (Time.timeScale == Settings.options.timeScale)
					{
						slowdownLerp = 0f;
					}
					float num4 = 2f;
					if (interrupted)
					{
						num4 = 0.3f;
					}
					Time.timeScale = (GameManager.m_GlobalTimeScale = Mathf.Lerp(Settings.options.timeScale, 1f, slowdownLerp += Time.unscaledDeltaTime / num4));
					MelonLogger.Msg("closed " + Time.timeScale);
				}
				else if ((int)handsAnim.updateMode > 0)
				{
					handsAnim.updateMode = (AnimatorUpdateMode)0;
					timescaleReset = true;
				}
			}
			return;
			IL_040a:
			StopOnBookClose();
			if (InputManager.GetRotateClockwiseHeld(InputManager.m_CurrentContext) && !flag && !flag2)
			{
				if (currentState != "open" || PageManager.currentPage + 1 >= PageManager.splitPages.Length || PageManager.currentTurn != null)
				{
					return;
				}
				PageManager.currentTurn = "next";
				handsAnim.SetTrigger("next_page");
			}
			if (InputManager.GetRotateCounterClockwiseHeld(InputManager.m_CurrentContext) && !flag && !flag2)
			{
				if (currentState != "open" || PageManager.currentPage - 1 < 1 || PageManager.currentTurn != null)
				{
					return;
				}
				PageManager.currentTurn = "prev";
				handsAnim.SetTrigger("prev_page");
			}
			if (((Object)(object)GameManager.GetPlayerManagerComponent().m_ItemInHands != (Object)null || InputManager.HasInteractedThisFrame() || GameManager.GetPlayerStruggleComponent().InStruggle()) && currentState != "pocket")
			{
				handsAnim.SetTrigger("remove_book");
				currentState = "pocket";
				interrupted = true;
			}
			if (InputManager.GetHolsterPressed(InputManager.m_CurrentContext) || flag || flag2)
			{
				if (InterfaceManager.IsOverlayActiveCached())
				{
					return;
				}
				if (currentState == "title")
				{
					handsAnim.SetTrigger("remove_book");
					currentState = "pocket";
				}
				if (currentState == "open")
				{
					handsAnim.SetTrigger("close_book");
					currentState = "pocket";
				}
			}
			goto IL_0606;
		}
	}
	internal static class Settings
	{
		public static ReadSettings options;

		public static void OnLoad()
		{
			options = new ReadSettings();
			((ModSettingsBase)options).AddToModSettings("悠闲阅读 设置");
		}
	}
	internal class ReadSettings : JsonModSettings
	{
		[Section("常规设置")]
		[Name("当前页")]
		[Description("当前页。用于快速翻阅书籍。\n\n目前无法获取书籍的实际页数,因此上限固定为 999。")]
		[Slider(1f, 999f, 500)]
		public int currentPage = 1;

		[Name("字体大小")]
		[Description("页面字体大小。默认:16")]
		[Slider(14f, 24f)]
		public int fontSize = 16;

		[Name("RTL 语言(从右至左)")]
		[Description("启用从右至左的书写方式。\n\n该选项还会启用阿拉伯字符变形,并让书本向右翻开。\n\n支持波斯语、阿拉伯语和希伯来语。\n\n开启后从左至右的语言会显示异常,无法兼顾。")]
		public bool enableRTL = false;

		[Name("文字对齐")]
		[Description("默认:两端对齐。\n\n如果你的语言用两端对齐效果不佳,或者想在读诗歌时显得风雅,可以更换。")]
		[Choice(new string[] { "两端对齐", "左对齐", "右对齐", "居中" })]
		public int textAlignment;

		[Name("时间减速")]
		[Description("阅读时减慢时间流逝,让你更沉浸于故事。不影响野生动物。\n\n默认:1")]
		[Slider(0.1f, 1f, 10)]
		public float timeScale = 1f;

		[Name("禁用交互")]
		[Description("阅读时禁用交互。当你越过书本朝外看时仍可交互。\n\n搭配经典倾斜模式效果不好,因为无法真正越过书本看外面。\n\n由于默认启用了新的倾斜模式,默认:开启")]
		public bool disableInteraction = true;

		[Section("控制")]
		[Name("快捷键")]
		[Description("按下该键可取出书本,再按一次打开。用左/右按键(Q 和 E)翻页。按收起武器键关闭。\n\n默认:5")]
		public KeyCode openKeyCode = (KeyCode)53;

		[Name("书本倾斜")]
		[Description("根据鼠标 Y 轴移动改变书本倾斜。新模式让你向前看时视野更广。\n\n默认:新模式")]
		[Choice(new string[] { "新模式", "经典" })]
		public int bookTilt;

		[Section("自定义")]
		[Name("书本贴图")]
		[Description("书本外观。\n\n你可以自行修改贴图,文件位置:...Mods/pastimeReading/textures/")]
		[Choice(new string[] { "纯色红", "纯色蓝", "纯色白", "纯色黄", "细节灰", "细节黄", "细节黑" })]
		public int bookTexture;

		[Section("重载")]
		[Name("从文本文件重新加载书籍?")]
		[Description("勾选后,按下确认时将从文件重新加载书籍。")]
		public bool reloadBook;

		[Section("调试")]
		[Name("启用调试日志")]
		[Description("")]
		public bool debugLog = false;

		public static bool settingsChanged;

		protected override void OnConfirm()
		{
			settingsChanged = true;
			((JsonModSettings)this).OnConfirm();
			if (!((Object)(object)ReadMain.hands == (Object)null))
			{
				if (PageManager.currentlyRTL != Settings.options.enableRTL)
				{
					PageManager.InitPages("rtl");
					Settings.options.reloadBook = true;
				}
				if (Settings.options.reloadBook)
				{
					PageManager.InitPages("read");
					PageManager.InitPages("split");
					Settings.options.currentPage = 1;
					PageManager.InitPages("setup");
					Settings.options.reloadBook = false;
				}
				if (PageManager.currentBookTexture != Settings.options.bookTexture)
				{
					ReadInstance.UpdateBookTexture();
					PageManager.InitPages("setup");
				}
				if (PageManager.currentPage != Settings.options.currentPage)
				{
					PageManager.currentPage = Settings.options.currentPage;
					PageManager.InitPages("page");
					PageManager.InitPages("setup");
				}
				if (PageManager.currentFontSize != (float)Settings.options.fontSize)
				{
					PageManager.InitPages("font");
					PageManager.InitPages("split");
					PageManager.InitPages("page");
					PageManager.InitPages("setup");
				}
				if (PageManager.currentAlignment != Settings.options.textAlignment)
				{
					PageManager.InitPages("font");
				}
				if (ReadMain.bookIsOpened)
				{
					Time.timeScale = Settings.options.timeScale;
					GameManager.m_GlobalTimeScale = Settings.options.timeScale;
					ReadMain.handsAnim.updateMode = (AnimatorUpdateMode)((Settings.options.timeScale != 1f) ? 2 : 0);
				}
			}
		}
	}
	public static class SoundManager
	{
		private static string stateLast;

		public static bool initDone;

		public static void InitSounds()
		{
			initDone = true;
		}

		public static void AnimatorStateDJ(string state, string wwiseEvent)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			AnimatorStateInfo currentAnimatorStateInfo = ReadMain.handsAnim.GetCurrentAnimatorStateInfo(0);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName(state))
			{
				if (stateLast != state)
				{
					AkSoundEngine.PostEvent(wwiseEvent, ReadMain.hands);
				}
				stateLast = state;
			}
			else if (stateLast == state)
			{
				stateLast = null;
			}
		}

		public static void AnimatorStateDJAC(string animState, AudioClip clip)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			AnimatorStateInfo currentAnimatorStateInfo = ReadMain.handsAnim.GetCurrentAnimatorStateInfo(0);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName(animState))
			{
				if (stateLast != animState)
				{
				}
				stateLast = animState;
			}
			else if (stateLast == animState)
			{
				stateLast = null;
			}
		}
	}
	[HarmonyPatch(typeof(PlayerManager), "ShouldSuppressCrosshairs")]
	public class HideCrosshairWhileUsingBook
	{
		private static void Postfix(ref bool __result)
		{
			__result = __result || (ReadMain.lockInteraction && Settings.options.disableInteraction);
		}
	}
	[HarmonyPatch(typeof(BaseAi), "Update")]
	public class ResetWildLifeAnimatorsWhileSlowdown
	{
		private static void Postfix(BaseAi __instance)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Invalid comparison between Unknown and I4
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Invalid comparison between Unknown and I4
			if (ReadMain.bookIsOpened)
			{
				if ((int)__instance.m_Animator.updateMode != 2)
				{
					__instance.m_Animator.updateMode = (AnimatorUpdateMode)2;
				}
			}
			else if ((int)__instance.m_Animator.updateMode > 0)
			{
				__instance.m_Animator.updateMode = (AnimatorUpdateMode)0;
			}
		}
	}
	public class Utility
	{
		public static bool IsScenePlayable()
		{
			return !string.IsNullOrEmpty(GameManager.m_ActiveScene) && !GameManager.m_ActiveScene.Contains("MainMenu") && !(GameManager.m_ActiveScene == "Boot") && !(GameManager.m_ActiveScene == "Empty");
		}

		public static bool IsScenePlayable(string scene)
		{
			return !string.IsNullOrEmpty(scene) && !scene.Contains("MainMenu") && !(scene == "Boot") && !(scene == "Empty");
		}

		public static void Log(ConsoleColor color, string message)
		{
			if (Settings.options.debugLog)
			{
				MelonLogger.Msg(color, message);
			}
		}
	}
}
