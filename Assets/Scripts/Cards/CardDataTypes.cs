using System;

[Serializable]
public class CardData
{
    public int id;
    public string name;
    public int cost;
    public int power;
    public CardAbilityData ability;
}

[Serializable]
public class CardAbilityData
{
    public string type;
    public int value;
}

[Serializable]
public class CardList
{
    public CardData[] cards;
}