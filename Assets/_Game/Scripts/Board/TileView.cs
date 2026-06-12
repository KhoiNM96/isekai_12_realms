using Isekai12Realms.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Board
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class TileView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Color selectedColor = Color.white;

        private Color baseColor;
        private BoardController boardController;

        public TileData Data { get; private set; }

        public void Initialize(BoardController controller, TileData data)
        {
            boardController = controller;
            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (label == null)
            {
                label = GetComponentInChildren<TextMeshProUGUI>();
            }

            Button button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
            Bind(data);
        }

        public void Bind(TileData data)
        {
            Data = data;
            transform.name = $"Tile_{data.position.x}_{data.position.y}";
            baseColor = GetColor(data.type);
            background.color = baseColor;
            if (label != null)
            {
                label.text = GetLabel(data);
                label.color = Color.white;
            }
        }

        public void SetSelected(bool selected)
        {
            background.color = selected ? Color.Lerp(baseColor, selectedColor, 0.55f) : baseColor;
        }

        public void RefreshPosition()
        {
            if (Data != null)
            {
                transform.name = $"Tile_{Data.position.x}_{Data.position.y}";
            }
        }

        private void OnClicked()
        {
            boardController?.TrySelectTile(this);
        }

        private static string GetLabel(TileData data)
        {
            if (data.specialType == SpecialTileType.RealmCrystal) return "C";
            if (data.specialType == SpecialTileType.BombRune) return "B";
            if (data.specialType == SpecialTileType.RowRune) return "R";
            if (data.specialType == SpecialTileType.ColumnRune) return "L";
            return data.type.ToString().Substring(0, 1);
        }

        private static Color GetColor(TileType type)
        {
            switch (type)
            {
                case TileType.Sword: return new Color(0.25f, 0.8f, 0.82f, 1f);
                case TileType.Heart: return new Color(1f, 0.28f, 0.38f, 1f);
                case TileType.Coin: return new Color(1f, 0.78f, 0.22f, 1f);
                case TileType.Food: return new Color(0.45f, 0.85f, 0.32f, 1f);
                case TileType.Book: return new Color(0.62f, 0.42f, 0.95f, 1f);
                case TileType.Mana: return new Color(0.32f, 0.52f, 1f, 1f);
                case TileType.Shield: return new Color(0.58f, 0.64f, 0.72f, 1f);
                case TileType.Star: return new Color(1f, 0.92f, 0.25f, 1f);
                default: return Color.gray;
            }
        }
    }
}
