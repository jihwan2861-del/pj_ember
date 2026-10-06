using UnityEngine;

namespace EmberPrototype
{
    // Only placed in the isolated rope example scene.
    public sealed class RopeValidationHud : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.Box(new Rect(16f, 16f, 500f, 105f),
                "EMBER - Rope validation\n" +
                "Arrows: move / swing    X: enter burning end\n" +
                "Z in torch: aimed launch    Z in rope: kick along swing\n" +
                "Torch -> Rope 1 -> Rope 2 -> Landing    R: restart");
        }
    }
}
