using UnityEngine;

namespace EmberPrototype
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        private void OnGUI()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.55f, 0.12f) }
            };
            bodyStyle ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 16,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(20, 16, 500, 34), "EMBER — Minimum Fire Prototype", titleStyle);
            GUI.Box(new Rect(20, 54, 610, 175),
                "R : Restart room (resets fire)\n" +
                "Arrow Keys : Move / Aim     Z : Jump / Double Jump\n" +
                "X : Absorb into a burning tile in the aimed direction\n" +
                "C : Ignite nearby flammable objects (or interact at an altar)\n" +
                "Inside fire, Arrow Keys : Aim at another burning fire\n" +
                "Inside fire, X : Move to the highlighted fire\n" +
                "Inside fire, Arrow Keys + Z : Launch\n" +
                "Brown = flammable   Orange = burning   Gray = non-flammable", bodyStyle);
        }
    }
}
