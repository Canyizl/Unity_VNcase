using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public RectTransform container;
    public CharacterDisplay displayPrefab;
    public CharacterData[] allCharacters;

    private List<CharacterDisplay> pool = new List<CharacterDisplay>();
    private Dictionary<string, CharacterDisplay> activeDisplays = new Dictionary<string, CharacterDisplay>();

    public void ShowCharacter(string characterID, Vector2? position = null, string expressionKey = null)
    {
        var data = allCharacters.FirstOrDefault(c => c.characterID == characterID);
        if (data == null)
        {
            return;
        }

        Sprite spriteToUse = data.standSprite;
        if (expressionKey != null && data.expressions.ContainsKey(expressionKey))
        {
            spriteToUse = data.expressions[expressionKey];
        }
        if (activeDisplays.TryGetValue(characterID, out var display))
        {
            display.Setup(spriteToUse, position ?? data.defaultPosition, data.defaultScale);
            return;
        }

        var available = pool.FirstOrDefault(d => !d.gameObject.activeSelf);
        if (available == null)
        {
            available = Instantiate(displayPrefab, container);
            pool.Add(available);
        }

        available.Setup(spriteToUse, position ?? data.defaultPosition, data.defaultScale);
        activeDisplays[characterID] = available;
    }

    public void HideCharacter(string characterID)
    {
        if (activeDisplays.TryGetValue(characterID, out var display))
        {
            display.gameObject.SetActive(false);
            activeDisplays.Remove(characterID);
        }
    }
    public void ClearAll()
    {
        foreach (var pair in activeDisplays)
            pair.Value.gameObject.SetActive(false);
        activeDisplays.Clear();
    }
}
