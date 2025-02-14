using UnityEditor;
using UnityEngine;

namespace Script
{
    public class ToolsForBasicsTree : EditorWindow
    {
        private float brushSize;
        private float maxBrushSize = 256;
        private float minBrushSize = 1;
    
        private float density;
        private float minDensity = 1;
        private float maxDensity = 100;
    
        private GameObject treePrefab;

        private string baseName;
        private int objectId = 1;
    
        [MenuItem("Tools/Tree Tool")]
        private static void ShowWindow()
        {
            GetWindow(typeof(ToolsForBasicsTree));
        }

        private void OnGUI()
        {
            GUILayout.Label("Tree Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            treePrefab = EditorGUILayout.ObjectField("Tree Prefabs",treePrefab,typeof(GameObject),false) as GameObject;
            EditorGUILayout.Space();
            brushSize = EditorGUILayout.Slider("Brush Size", brushSize,minBrushSize, maxBrushSize);
            EditorGUILayout.Space();
            density = EditorGUILayout.Slider("Density", density, minDensity, maxDensity);

            if (GUILayout.Button("Paint Tree"))
            {
                SceneView.duringSceneGui += OnSceneGUI;
            }

            if (GUILayout.Button("Stop Painting Tree"))
            {
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {   
            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Handles.color = new Color(0, 1, 0, 0.5f);
                Handles.DrawWireDisc(hit.point,hit.normal,brushSize);
            
                if (e.type == EventType.MouseDown && e.button == 0 && treePrefab != null)
                {
                    Undo.SetCurrentGroupName("Paint Trees");
                    for (int i = 0; i < density; i++)
                    {
                        Vector2 randomOffset = Random.insideUnitCircle * brushSize;
                        Vector3 position = hit.point + new Vector3(randomOffset.x,0 ,randomOffset.y);

                        if (Physics.Raycast(position,Vector3.down,out RaycastHit groundHit))
                        {
                        
                            position.y = groundHit.point.y;
                        
                            if (Vector3.Angle(Vector3.up, hit.normal) < 30f)  // Ensure surface isn't too steep
                            {
                                GameObject tree = PrefabUtility.InstantiatePrefab(treePrefab) as GameObject;
                                tree.transform.position = position;
                                tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0); // pas con pour que il n'y est pas un effet de redondance
                                Undo.RegisterCreatedObjectUndo(tree, "Paint Tree"); // jp ctrl + z
                            }
                        }
                    
                    }
                
                }
            }
        
            sceneView.Repaint();
        }
   
    }
}