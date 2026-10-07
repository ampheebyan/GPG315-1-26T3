using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UIElements.Slider;

namespace Editor
{
    public class ObjectPopulatorinator : EditorWindow
    {
        private int _padding = 10;
        
        private ObjectField _objectField;
        private Vector3Field _originField;
        private Slider _densitySlider;
        private Slider _radiusSlider;
        private Label _statusLabel;
        
        [MenuItem("Tools/Object Populatorinator")]
        private static void ShowWindow()
        {
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
            
            _originField = new Vector3Field("Origin Point");
            rootVisualElement.Add(_originField);
            
            _objectField = new ObjectField("Prefab")
            {
                allowSceneObjects = false,
                objectType = typeof(GameObject)
            };
            rootVisualElement.Add(_objectField);
            
            _radiusSlider = new Slider("Radius", 1f, 150f)
            {
                showInputField = true
            };
            _radiusSlider.RegisterValueChangedCallback(Callback);
            rootVisualElement.Add(_radiusSlider);
            
            _densitySlider = new Slider("Density", .1f, 1f)
            {
                showInputField = true
            };
            _densitySlider.RegisterValueChangedCallback(Callback);
            rootVisualElement.Add(_densitySlider);
            Button trigger = new Button(ClickEvent)
            {
                text = "Populate"
            };
            rootVisualElement.Add(trigger);
            
            _statusLabel = new Label("");
            rootVisualElement.Add(_statusLabel);
        }

        private void Callback(ChangeEvent<float> evt)
        {
            float area = Mathf.PI * (_radiusSlider.value * _radiusSlider.value);
            float objects = Mathf.RoundToInt(area * _densitySlider.value);
            Debug.Log($"{_densitySlider.value}, {_radiusSlider.value}: {objects}");
            _statusLabel.text = objects >= 500 ? $"WARNING: {objects} objects will be created!" : "";
        }

        private void ClickEvent()
        {
            if (_objectField.value == null)
            {
                Debug.Log("no object selected");
                _statusLabel.text = "No object!";
                return;
            }
            // clear if no issues 
            _statusLabel.text = "";
            Debug.Log($"{_objectField.value.name}, origin: {_originField.value}, radius: {_radiusSlider.value}, density: {_densitySlider.value}");
            
            float area = Mathf.PI * (_radiusSlider.value * _radiusSlider.value);
            float objects = Mathf.RoundToInt(area * _densitySlider.value);
            Debug.Log($"creating {objects} clones of {_objectField.value.name}, prepare for the worst!");
            _statusLabel.text = $"Creating {objects} clones of {_objectField.value.name}, prepare for the worst!";
            // choosing the lazy way out with undo handling (making a container object and then just registering that for the undo. like i would do this anyway for neatness but like, this is just an extra bonus!)
            GameObject populationContainer = new GameObject($"{_objectField.value.name} ({objects}, radius: {_radiusSlider.value}, density: {_densitySlider.value})");
            Undo.RegisterCreatedObjectUndo(populationContainer, $"Clone {_objectField.value.name} {objects} times.");
            for (int i = 0; i < objects; i++)
            {
                Vector2 rPos = Random.insideUnitCircle * _radiusSlider.value;
                GameObject instancedObject = Instantiate(_objectField.value, _originField.value + new Vector3(rPos.x, 0f, rPos.y), Quaternion.identity) as GameObject;
                if (instancedObject != null)
                {
                    instancedObject.transform.SetParent(populationContainer.transform);
                    instancedObject.name = $"{_objectField.value.name} {i}";
                }
            }
            _statusLabel.text = $"Created {objects} clones of {_objectField.value.name}.";
        }
    }
}