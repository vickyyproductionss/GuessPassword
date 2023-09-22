using UnityEngine;
using VickyCustoms;

[CreateAssetMenu(fileName = "Chest", menuName = "Custom Chests/Chest")]
public class ChestType : ScriptableObject
{
	public Sprite ChestImage;
	public string ChestName;
	public string PrizeMoney;
	public string DigitsToUnlock;
	public LockerDigits _lockerDigits;
	public int TotalPossiblePasswords;
	public int StartingPrize;
	public int MaximumPrize;
}
