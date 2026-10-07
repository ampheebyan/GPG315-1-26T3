using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UIElements.Slider;

namespace Editor
{
    public class ObjectPopulatorinatorSettings : ScriptableObject
    {
        public Vector3 Origin;
        public GameObject Prefab;
        public float Density = 0.1f;
        public float Radius = 30f;
    }
    public class ObjectPopulatorinator : EditorWindow
    {
        private readonly int _padding = 10;
        private ObjectPopulatorinatorSettings _settings;
        private ObjectField _objectField;
        private Vector3Field _originField;
        private Slider _densitySlider;
        private Slider _radiusSlider;
        private Label _statusLabel;

        private void OnEnable()
        {
            Debug.Log($"ObjectPopulatorinator: OnEnable");
            _settings = CreateInstance<ObjectPopulatorinatorSettings>();
            SceneView.duringSceneGui += SceneViewOnduringSceneGui;
            Undo.undoRedoEvent += UndoRedoEvent;
        }

        private void UndoRedoEvent(in UndoRedoInfo undo)
        {
            // Literally trying everything to get this to repaint
            // https://docs.unity3d.com/2019.1/Documentation/ScriptReference/UI.Slider.SetValueWithoutNotify.html
            _originField.SetValueWithoutNotify(_settings.Origin);
            _objectField.SetValueWithoutNotify(_settings.Prefab);
            _densitySlider.SetValueWithoutNotify(_settings.Density);
            _radiusSlider.SetValueWithoutNotify(_settings.Radius);
            Callback();
            Repaint();
        }

        private void OnDisable()
        {
            Debug.Log($"ObjectPopulatorinator: OnDisable");
            SceneView.duringSceneGui -= SceneViewOnduringSceneGui;
            Undo.undoRedoEvent -= UndoRedoEvent;
            DestroyImmediate(_settings);
        }

        [MenuItem("Tools/Object Populatorinator")]
        private static void ShowWindow()
        {
            Debug.Log($"ObjectPopulatorinator: ShowWindow");

            var window = GetWindow<ObjectPopulatorinator>();
            window.titleContent = new GUIContent("Object Populatorinator");
            window.Show();
        }

        private void CreateGUI()
        {
            // wow this is really just css huh. not sure if i hate it or like it
            rootVisualElement.style.paddingBottom = _padding;
            rootVisualElement.style.paddingTop = _padding;
            rootVisualElement.style.paddingLeft = _padding;
            rootVisualElement.style.paddingRight = _padding;
            // have never really made my own tools that use the editor in this way before, so this is going to be bad and awful, but that's what learning is for, right?
            Label titleLabel = new Label("<size=150%>The Object Populatorinator</size>");
            Label label = new Label("Created by Phoebe B. (1033478) for GPG315.1");
            rootVisualElement.Add(titleLabel);
            rootVisualElement.Add(label);
            
            _originField = new Vector3Field("Origin Point")
            {
                value = _settings.Origin
            };

            _originField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Origin Point Change");
                _settings.Origin = evt.newValue;
                Repaint();
            });
            rootVisualElement.Add(_originField);
            
            _objectField = new ObjectField("Prefab")
            {
                allowSceneObjects = false,
                objectType = typeof(GameObject),
                value = _settings.Prefab
            };
            _objectField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Prefab Change");
                _settings.Prefab = (GameObject)evt.newValue;
                Repaint();
            });
            rootVisualElement.Add(_objectField);
            
            _radiusSlider = new Slider("Radius", 1f, 150f)
            {
                showInputField = true,
                value = _settings.Radius
            };
            _radiusSlider.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Radius Change");
                _settings.Radius = evt.newValue;
                Callback();
                Repaint();
            });
            rootVisualElement.Add(_radiusSlider);
            
            _densitySlider = new Slider("Density", .1f, 1f)
            {
                showInputField = true,
                value = _settings.Density
            };
            _densitySlider.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_settings, "Density Change");
                _settings.Density = evt.newValue;
                Callback();
                Repaint();
            });
            rootVisualElement.Add(_densitySlider);
            Button trigger = new Button(ClickEvent)
            {
                text = "Populate"
            };
            rootVisualElement.Add(trigger);
            
            _statusLabel = new Label("");
            rootVisualElement.Add(_statusLabel);
        }

        private void SceneViewOnduringSceneGui(SceneView obj)
        {
            Handles.color = Color.mediumPurple;
            EditorGUI.BeginChangeCheck();
            Vector3 pos = Handles.PositionHandle(_settings.Origin, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_settings, "Origin Point Change");
                _settings.Origin = pos;
                _originField.SetValueWithoutNotify(pos);
                Repaint();
            }
        }

        private Vector2 CalculateAreaObjects()
        {
            float area = Mathf.PI * (_settings.Radius * _settings.Radius);
            float objects = Mathf.RoundToInt(area * _settings.Density);
            return new Vector2(objects, area);
        }
        
        private void Callback()
        {
            Vector2 areaObjects = CalculateAreaObjects();
            Debug.Log($"{_settings.Density}, {_settings.Radius}: {areaObjects.x}");
            _statusLabel.text = areaObjects.x >= 500 ? $"<color=#b5a435><b>WARNING</b></color>: {areaObjects.x} objects will be created!" : "";
        }

        private void ClickEvent()
        {
            if (_settings.Prefab == null)
            {
                Debug.Log("no object selected");
                _statusLabel.text = "<color=#b53535><b>No object!</b></color>";
                return;
            }
            // clear if no issues 
            _statusLabel.text = "";
            Debug.Log($"{_settings.Prefab.name}, origin: {_settings.Origin}, radius: {_settings.Radius}, density: {_settings.Density}");
            
            Vector2 areaObjects = CalculateAreaObjects();

            Debug.Log($"creating {areaObjects.x} clones of {_settings.Prefab.name}, prepare for the worst!");
            _statusLabel.text = $"Creating {areaObjects.x} clones of {_settings.Prefab.name}, prepare for the worst!";
            // choosing the lazy way out with undo handling (making a container object and then just registering that for the undo. like i would do this anyway for neatness but like, this is just an extra bonus!)
            GameObject populationContainer = new GameObject($"{_settings.Prefab.name} ({areaObjects.x}, radius: {_settings.Radius}, density: {_settings.Density})");
            Undo.RegisterCreatedObjectUndo(populationContainer, $"Clone {_settings.Prefab.name} {areaObjects.x} times.");
            for (int i = 0; i < areaObjects.x; i++)
            {
                Vector2 rPos = Random.insideUnitCircle * _settings.Radius;
                GameObject instancedObject = Instantiate(_settings.Prefab, _settings.Origin + new Vector3(rPos.x, 0f, rPos.y), Quaternion.identity);
                if (instancedObject != null)
                {
                    instancedObject.transform.SetParent(populationContainer.transform);
                    instancedObject.name = $"{_settings.Prefab.name} {i}";
                }
            }
            _statusLabel.text = $"Created {areaObjects.x} clones of {_settings.Prefab.name}.";
        }
    }
}