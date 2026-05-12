using System.Collections.Generic;
using System.Text;
using Cpp2ILInjected;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[Token(Token = "0x2000216")]
public class ClothingSlot : MonoBehaviour
{
	[Token(Token = "0x40012B7")]
	[FieldOffset(Offset = "0x20")]
	public UITexture m_TextureGearItem;

	[Token(Token = "0x40012B8")]
	[FieldOffset(Offset = "0x28")]
	public UISprite m_SpriteEmptySlot;

	[Token(Token = "0x40012B9")]
	[FieldOffset(Offset = "0x30")]
	public GameObject m_Selected;

	[Token(Token = "0x40012BA")]
	[FieldOffset(Offset = "0x38")]
	public UISprite m_SpriteWet;

	[Token(Token = "0x40012BB")]
	[FieldOffset(Offset = "0x40")]
	public UISprite m_SpriteFrozen;

	[Token(Token = "0x40012BC")]
	[FieldOffset(Offset = "0x48")]
	public UISprite m_SpriteOptionsAvailable;

	[Token(Token = "0x40012BD")]
	[FieldOffset(Offset = "0x50")]
	public UILabel m_LayerLabel;

	[Token(Token = "0x40012BE")]
	[FieldOffset(Offset = "0x58")]
	public GameObject m_SpriteBoxHover;

	[Token(Token = "0x40012BF")]
	[FieldOffset(Offset = "0x60")]
	public Material m_MaterialSource;

	[HideInInspector]
	[Token(Token = "0x40012C0")]
	[FieldOffset(Offset = "0x68")]
	public GearItem m_GearItem;

	[Token(Token = "0x40012C1")]
	[FieldOffset(Offset = "0x70")]
	private ClothingRegion m_ClothingRegion;

	[Token(Token = "0x40012C2")]
	[FieldOffset(Offset = "0x74")]
	private ClothingLayer m_ClothingLayer;

	[Token(Token = "0x40012C3")]
	[FieldOffset(Offset = "0x78")]
	private int m_LayoutColumnIndex;

	[Token(Token = "0x40012C4")]
	[FieldOffset(Offset = "0x7C")]
	private int m_LayoutRowIndex;

	[Token(Token = "0x40012C5")]
	[FieldOffset(Offset = "0x80")]
	private bool m_HasRegistered;

	[Token(Token = "0x40012C6")]
	[FieldOffset(Offset = "0x81")]
	private bool m_IsUsingAltTexture;

	[Token(Token = "0x40012C7")]
	[FieldOffset(Offset = "0x82")]
	private bool m_IsUsingThirdAltTexture;

	[Token(Token = "0x40012C8")]
	[FieldOffset(Offset = "0x84")]
	private VoicePersona m_VoiceInUse;

	[Token(Token = "0x40012C9")]
	[FieldOffset(Offset = "0x88")]
	private UITexture[] m_PaperDollSlots;

	[Token(Token = "0x40012CA")]
	[FieldOffset(Offset = "0x90")]
	private List<UIWidget> m_PaperDollSlotWidgets;

	[Token(Token = "0x40012CB")]
	[FieldOffset(Offset = "0x98")]
	private StringBuilder m_StringBuilder;

	[Token(Token = "0x40012CC")]
	[FieldOffset(Offset = "0xA0")]
	private bool m_HasAltLayoutIndex;

	[Token(Token = "0x40012CD")]
	[FieldOffset(Offset = "0xA4")]
	private int m_AltLayoutColumnIndex;

	[Token(Token = "0x40012CE")]
	[FieldOffset(Offset = "0xA8")]
	private int m_AltLayoutRowIndex;

	[Token(Token = "0x40012CF")]
	[FieldOffset(Offset = "0xB0")]
	public HoverWidgetControl m_HoverWidgetControl;

	[Token(Token = "0x40012D0")]
	[FieldOffset(Offset = "0xB8")]
	private Panel_Clothing m_Panel_Clothing;

	[Token(Token = "0x40012D1")]
	[FieldOffset(Offset = "0xC0")]
	private AsyncOperationHandle<Texture2D> m_MainTexLoadOperation;

	[Token(Token = "0x40012D2")]
	[FieldOffset(Offset = "0xD8")]
	private AsyncOperationHandle<Texture2D> m_DamageTexLoadOperation;

	[Token(Token = "0x40012D3")]
	[FieldOffset(Offset = "0xF0")]
	private AsyncOperationHandle<Texture2D> m_BlendTexLoadOperation;

	[Token(Token = "0x40012D4")]
	private const string PAPERDOLL_MASK_SHADER_KEYWORD = "TLD_PAPERDOLL_MASK";

	[Token(Token = "0x40012D5")]
	private const string MAIN_TEXTURE_NAME = "_MainTex";

	[Token(Token = "0x40012D6")]
	private const string DAMAGE_TEXTURE_NAME = "_DamageTex";

	[Token(Token = "0x40012D7")]
	private const string BLENDMAP_TEXTURE_NAME = "_blendmap";

	[Token(Token = "0x40012D8")]
	private static readonly int s_PaperdollMaskTextureShaderID;

	[Token(Token = "0x60012B0")]
	[Address(RVA = "0x6578E0", Offset = "0x6566E0", Length = "0x4B3")]
	public void DoSetup(Panel_Clothing parentPanel, string emptySlotSpriteName, ClothingRegion region, ClothingLayer layer, int column, int row, UITexture[] paperDolls, string layerLocID)
	{
	}

	[Token(Token = "0x60012B1")]
	[Address(RVA = "0x657DA0", Offset = "0x656BA0", Length = "0x100")]
	public void ActivateMouseHoverHighlight(bool isEnabled)
	{
	}

	[Token(Token = "0x60012B2")]
	[Address(RVA = "0x657EB0", Offset = "0x656CB0", Length = "0x15")]
	public void DoAltLayoutSetup(int column, int row)
	{
	}

	[Token(Token = "0x60012B3")]
	[Address(RVA = "0x657ED0", Offset = "0x656CD0", Length = "0x8")]
	public bool HasAltIndex()
	{
		return false;
	}

	[Token(Token = "0x60012B4")]
	[Address(RVA = "0x657EE0", Offset = "0x656CE0", Length = "0x4")]
	public int GetColumnIndex()
	{
		return 0;
	}

	[Token(Token = "0x60012B5")]
	[Address(RVA = "0x657EF0", Offset = "0x656CF0", Length = "0x13")]
	public bool ColumnMatchesLayout(int col)
	{
		return false;
	}

	[Token(Token = "0x60012B6")]
	[Address(RVA = "0x657F10", Offset = "0x656D10", Length = "0x91")]
	public void SetSelected(bool isSelected)
	{
	}

	[Token(Token = "0x60012B7")]
	[Address(RVA = "0x657FB0", Offset = "0x656DB0", Length = "0x5B")]
	public void DoClickAction()
	{
	}

	[Token(Token = "0x60012B8")]
	[Address(RVA = "0x658010", Offset = "0x656E10", Length = "0x4F")]
	public void DoDoubleClickAction()
	{
	}

	[Token(Token = "0x60012B9")]
	[Address(RVA = "0x658060", Offset = "0x656E60", Length = "0x2B")]
	public bool IsAtPositionInLayout(int col, int row)
	{
		return false;
	}

	[Token(Token = "0x60012BA")]
	[Address(RVA = "0x658090", Offset = "0x656E90", Length = "0x99")]
	public bool IsSelected()
	{
		return false;
	}

	[Token(Token = "0x60012BB")]
	[Address(RVA = "0x658130", Offset = "0x656F30", Length = "0x4")]
	public ClothingRegion GetClothingRegion()
	{
		return default(ClothingRegion);
	}

	[Token(Token = "0x60012BC")]
	[Address(RVA = "0x658140", Offset = "0x656F40", Length = "0x4")]
	public ClothingLayer GetClothingLayer()
	{
		return default(ClothingLayer);
	}

	[Token(Token = "0x60012BD")]
	[Address(RVA = "0x658150", Offset = "0x656F50", Length = "0x604")]
	public void UpdateSlotInfo()
	{
	}

	[Token(Token = "0x60012BE")]
	[Address(RVA = "0x658760", Offset = "0x657560", Length = "0x605")]
	public void UpdateMaskingFromLayeredClothing()
	{
	}

	[Token(Token = "0x60012BF")]
	[Address(RVA = "0x658D70", Offset = "0x657B70", Length = "0x19D")]
	public void ToggleWidgetsActive(bool toggle)
	{
	}

	[Token(Token = "0x60012C0")]
	[Address(RVA = "0x658F10", Offset = "0x657D10", Length = "0x4")]
	public int GetLayoutRow()
	{
		return 0;
	}

	[Token(Token = "0x60012C1")]
	[Address(RVA = "0x657EE0", Offset = "0x656CE0", Length = "0x4")]
	public int GetLayoutColumn()
	{
		return 0;
	}

	[Token(Token = "0x60012C2")]
	[Address(RVA = "0x658F20", Offset = "0x657D20", Length = "0xB2")]
	private void UpdateWetFrozenSprites(ClothingItem clothingItem)
	{
	}

	[Token(Token = "0x60012C3")]
	[Address(RVA = "0x658FE0", Offset = "0x657DE0", Length = "0x14C")]
	private Texture GetPaperDollTexture(int slotIndex)
	{
		return null;
	}

	[Token(Token = "0x60012C4")]
	[Address(RVA = "0x659130", Offset = "0x657F30", Length = "0x378")]
	private void SetPaperDollTexture(GearItem gi)
	{
	}

	[Token(Token = "0x60012C5")]
	[Address(RVA = "0x6594B0", Offset = "0x6582B0", Length = "0x64")]
	private bool IsAddressableTexture(string name)
	{
		return false;
	}

	[Token(Token = "0x60012C6")]
	[Address(RVA = "0x659520", Offset = "0x658320", Length = "0x158")]
	private void AdjustForFemaleVariant()
	{
	}

	[Token(Token = "0x60012C7")]
	[Address(RVA = "0x659680", Offset = "0x658480", Length = "0x459")]
	private void ClearAndReleasePaperDollTextures()
	{
	}

	[Token(Token = "0x60012C8")]
	[Address(RVA = "0x659AE0", Offset = "0x6588E0", Length = "0x9DC")]
	private void SetPaperDollTextureBlendMap(GearItem gi, bool isFemale)
	{
	}

	[Token(Token = "0x60012C9")]
	[Address(RVA = "0x65A4C0", Offset = "0x6592C0", Length = "0x36B")]
	private void SetPaperDollTextureNoBlendMap(GearItem gi, bool isFemale)
	{
	}

	[Token(Token = "0x60012CA")]
	[Address(RVA = "0x65A830", Offset = "0x659630", Length = "0x426")]
	private void SetBlendAmountOnly(GearItem gi)
	{
	}

	[Token(Token = "0x60012CB")]
	[Address(RVA = "0x65AC60", Offset = "0x659A60", Length = "0x39C")]
	private bool HasOptionsAvailable()
	{
		return false;
	}

	[Token(Token = "0x60012CC")]
	[Address(RVA = "0x65B000", Offset = "0x659E00", Length = "0x43")]
	private int GetPaperDollTextureLayer()
	{
		return 0;
	}

	[Token(Token = "0x60012CD")]
	[Address(RVA = "0x65B050", Offset = "0x659E50", Length = "0x16F")]
	private void UpdatePaperDollTextureLayer(int newLayer)
	{
	}

	[Token(Token = "0x60012CE")]
	[Address(RVA = "0x65B1C0", Offset = "0x659FC0", Length = "0xAEF")]
	private bool ShouldUseAltTexture()
	{
		return false;
	}

	[Token(Token = "0x60012CF")]
	[Address(RVA = "0x65BCB0", Offset = "0x65AAB0", Length = "0x1B2")]
	private bool ShouldUseThirdAltTexture()
	{
		return false;
	}

	[Token(Token = "0x60012D0")]
	[Address(RVA = "0x65BE70", Offset = "0x65AC70", Length = "0x254")]
	private void CheckForChangeLayer()
	{
	}

	[Token(Token = "0x60012D1")]
	[Address(RVA = "0x65C0D0", Offset = "0x65AED0", Length = "0xB2")]
	private void UpdatePaperDollLayer_Hands()
	{
	}

	[Token(Token = "0x60012D2")]
	[Address(RVA = "0x65C190", Offset = "0x65AF90", Length = "0xB2")]
	private void UpdatePaperDollLayer_FeetTop()
	{
	}

	[Token(Token = "0x60012D3")]
	[Address(RVA = "0x65C250", Offset = "0x65B050", Length = "0x660")]
	private void UpdatePaperDollLayer_HeadMid()
	{
	}

	[Token(Token = "0x60012D4")]
	[Address(RVA = "0x65C8C0", Offset = "0x65B6C0", Length = "0x74E")]
	private void UpdatePaperDollLayer_Accessory(int CHEST_MID_DEFAULT_LAYER)
	{
	}

	[Token(Token = "0x60012D5")]
	[Address(RVA = "0x65D010", Offset = "0x65BE10", Length = "0x32E")]
	private void UpdatePaperDollLayer_Astrid(int CHEST_BASE_LAYER, int CHEST_MID_DEFAULT_LAYER)
	{
	}

	[Token(Token = "0x60012D6")]
	[Address(RVA = "0x65D340", Offset = "0x65C140", Length = "0x459")]
	private void CheckForHide()
	{
	}

	[Token(Token = "0x60012D7")]
	[Address(RVA = "0x65D7A0", Offset = "0x65C5A0", Length = "0xC6")]
	private static bool IsMatchingGearName(string a, string b)
	{
		return false;
	}

	[Token(Token = "0x60012D8")]
	[Address(RVA = "0x65D870", Offset = "0x65C670", Length = "0xF2")]
	public ClothingSlot()
	{
	}
}
