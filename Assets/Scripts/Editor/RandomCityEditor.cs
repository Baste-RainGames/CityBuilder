using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof (RandomCity))]
public class RandomCityEditor : UnityEditor.Editor {

    private RandomCity script;

    private void OnEnable() {
        script = (RandomCity) target;
    }

    public override void OnInspectorGUI() {
        script.testOutput = (TextAsset) EditorGUILayout.ObjectField("Test output file", script.testOutput, typeof (TextAsset), false);
        script.visualizer = (CityToBlocks) EditorGUILayout.ObjectField("Visualizer", script.visualizer, typeof (CityToBlocks), true);

        if (script.currentGenerator != null) {
            if (GUILayout.Button("Step city creation next")) {
                if (!script.currentGenerator.MoveNext()) {
                    script.currentGenerator = null;
                    EditorUtility.SetDirty(script);
                    return;
                }
                City generated = script.currentGenerator.Current;
                WriteToTextAsset.Write(script.testOutput, generated.ToString());
                script.visualizer.CreateBlocks();

            }
            if (GUILayout.Button("Finalize generation")) {
                City generated = null;
                while (script.currentGenerator.MoveNext()) {
                    generated = script.currentGenerator.Current;
                }
                WriteToTextAsset.Write(script.testOutput, generated.ToString());
                script.visualizer.CreateBlocks();
                script.currentGenerator = null;
            }

            if (GUILayout.Button("Bail out of generation")) {
                script.currentGenerator = null;
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("openings"), true);
            serializedObject.ApplyModifiedProperties();

            return;
        }

        if (GUILayout.Button("Write an empty city to the file")) {
            WriteToTextAsset.Write(script.testOutput, script.CreateEmptyCity().ToString());
            script.visualizer.CreateBlocks();
        }

        if (GUILayout.Button("Create a random city")) {
            WriteToTextAsset.Write(script.testOutput, script.StartCreatingRandomCity().ToString());
            script.visualizer.CreateBlocks();
        }

        if (GUILayout.Button("Create big city, insert random from sources at random point")) {
            City c = script.CreateEmptyCity(20, 20);

            for (int i = 0; i < 5; i++) {
                City toInsert = City.FromString(script.sources[Random.Range(0, script.sources.Length)]);
                int XInsert = Random.Range(0, 15);
                int YInsert = Random.Range(0, 15);
                c = c.Insert(toInsert, XInsert, YInsert);
            }

            WriteToTextAsset.Write(script.testOutput, c.ToString());
            script.visualizer.CreateBlocks();
        }

        GUILayout.Space(20);

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sources"), true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Sum all of the source")) {
            var representations = script.sources;
            City c = City.FromString(representations[0].text);
            for (int i = 1; i < representations.Length; i++) {
                c += City.FromString(representations[i].text);
            }

            WriteToTextAsset.Write(script.testOutput, c.ToString());
        }
    }
}
