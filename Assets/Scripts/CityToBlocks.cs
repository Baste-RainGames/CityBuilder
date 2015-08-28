using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class CityToBlocks : MonoBehaviour {

    public TextAsset city;
    public Material house, street, debug;

    public void CreateBlocks() {
        Transform[] allChildren = transform.GetComponentsInChildren<Transform>();
        for (int i = 1; i < allChildren.Length; i++) {
            DestroyImmediate(allChildren[i].gameObject);
        }

        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(city));

        CityTile[,] tiles = City.FromString(city).cityGrid;

        for (int x = 0; x < tiles.GetLength(0); x++) {
            for (int y = 0; y < tiles.GetLength(1); y++) {
                CityTile tileType = tiles[x, y];
                if (tileType == CityTile.Empty)
                    continue;

                Vector3 coordinates = new Vector3(x, 0, -y);

                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.position = coordinates;
                block.transform.parent = transform;

                if (tileType == CityTile.House) {
                    block.transform.localScale = new Vector3(1, 2, 1);
                    block.transform.position += Vector3.up * .5f;
                    block.GetComponent<MeshRenderer>().sharedMaterial = house;
                    block.name = "House";
                }
                else if (tileType == CityTile.Street) {
                    block.GetComponent<MeshRenderer>().sharedMaterial = street;
                    block.name = "Street";
                }
                else if (tileType == CityTile.DEBUG) {
                    block.GetComponent<MeshRenderer>().sharedMaterial = debug;
                    block.name = "DEBUG";
                }

                block.name += "(" + x + "," + y + ")";
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof (CityToBlocks))]
    public class CityToBlocksEditor : Editor {

        private CityToBlocks script;

        private void OnEnable() {
            script = (CityToBlocks) target;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Turn city into blocks")) {
                script.CreateBlocks();
            }
        }
    }

}
#endif
