using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor.UIToolkit {
    public class ManagedTabView : VisualElement {
        public const string Uxml = "Packages/github.neroweneed.commons/Editor/Resources/ManagedTabView.uxml";
        public const string Uss = "Packages/github.neroweneed.commons/Editor/Resources/ManagedTabView.uss";
        public const string ClassName = "managed-tab-view";
        public const string TabContainerClassName = "managed-tab-view--tab-container";
        public const string TabClassName = "managed-tab-view--tab";
        public const string SelectedTabClassName = "managed-tab-view--tab--selected";
        public const string UnselectedTabClassName = "managed-tab-view--tab--unselected";
        public const string HiddenClassName = "managed-tab-view--hidden";
        public const string SelectedClassName = "managed-tab-view--selected";
        public const string ContainerClassName = "managed-tab-view--container";
        private VisualElement container = null;
        private VisualElement tabContainer;

        public ManagedTabView() {
            Init();
        }

        public override VisualElement contentContainer => container ?? this;

        private void Init() {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml).CloneTree(this);
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Uss));
            container = this.Q<VisualElement>(null, ContainerClassName);
            tabContainer = this.Q<VisualElement>(null, TabContainerClassName);
            this.RegisterCallback<AttachToPanelEvent>(_ => Refresh());
        }
        public void Refresh() {
            tabContainer.Clear();
            foreach (var element in container.Children()) {
                var button = new Button()
                {
                    tabIndex = element.tabIndex,
                    text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(element.name)
                };
                button.clicked += () => UpdateTabView(button.tabIndex);
                button.AddToClassList(TabClassName);
                tabContainer.Add(button);

            }
            UpdateTabView();
        }
        private void UpdateTabView(int tabIndex = 0) {
            foreach (var element in container.Children()) {
                if (element.tabIndex == tabIndex) {
                    element.RemoveFromClassList(HiddenClassName);
                    element.AddToClassList(SelectedClassName);
                }
                else {
                    element.AddToClassList(HiddenClassName);
                    element.RemoveFromClassList(SelectedClassName);
                }
            }
            foreach (var element in tabContainer.Children()) {
                if (element.tabIndex == tabIndex) {
                    element.RemoveFromClassList(UnselectedTabClassName);
                    element.AddToClassList(SelectedTabClassName);
                }
                else {
                    element.AddToClassList(UnselectedTabClassName);
                    element.RemoveFromClassList(SelectedTabClassName);
                }
            }
        }
        public new class UxmlFactory : UxmlFactory<ManagedTabView, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
    }
}