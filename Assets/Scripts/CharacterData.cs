using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "VN/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterID;
    public Sprite standSprite;
    public Vector2 defaultPosition;
    public Vector2 defaultScale = Vector2.one;

    [System.Serializable]

    public class Expression
    {
        public string name;  // "happy", "sad"
        public Sprite sprite;
    }
    public List<Expression> expressionsList = new();
    public Dictionary<string, Sprite> expressions;

    private void OnEnable()
    {
        expressions = new Dictionary<string, Sprite>();
        foreach(var expr in expressionsList)
        {
            if (!expressions.ContainsKey(expr.name))
                expressions.Add(expr.name, expr.sprite);
        }
    }
}
