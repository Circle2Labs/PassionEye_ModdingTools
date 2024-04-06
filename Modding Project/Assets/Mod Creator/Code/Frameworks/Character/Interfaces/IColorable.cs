using UnityEngine;

namespace Code.Frameworks.Character.Interfaces
{
    public interface IColorable
    {
        public void SetColor(string fieldName, Color color, Material material);
        public Color GetColor(string fieldName, Material material);
    }
}
