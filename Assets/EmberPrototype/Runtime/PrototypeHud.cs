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
                "Arrow keys : Move / Aim     Z : Jump / Launch     X : Dash / Fire Travel\n" +
                "C : Ignite nearby fuel with a burst\n" +
                "In ignition ring, Z : Dash     Hold X : Aim ember shot\n" +
                "Inside fire, X : Move to the highlighted fire\n" +
                "Brown = flammable   Orange = burning   Gray = non-flammable", bodyStyle);
        }
    }
}
