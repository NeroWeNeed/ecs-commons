using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor.UIToolkit {
    public class ManagedListView : ListView {
        public const string Uxml = "Packages/github.neroweneed.commons/Editor/Resources/ManagedListViewCommands.uxml";
        public const string Uss = "Packages/github.neroweneed.commons/Editor/Resources/ManagedListViewCommands.uss";
        public const string CommandsContainerClassName = "unity-listview-commands-container";
        public const string CommandsClassName = "unity-listview-commands";
        public const string CommandClassName = "unity-listview-command";
        public const string AddCommandClassName = "unity-listview-command--add";
        public const string HiddenClassName = "unity-listview--hidden";
        public const string RemoveCommandClassName = "unity-listview-command--remove";
        private ScrollView scrollView;
        private VisualElement commands;
        private Button addButton;
        private Button removeButton;
        private Action addItem;
        public Action AddItem
        {
            get => addItem;
            set
            {
                addItem = value;
                RefreshButtons();
            }
        }
        private Action removeItem;
        public Action RemoveItem
        {
            get => removeItem;
            set
            {
                removeItem = value;
                RefreshButtons();
            }
        }
        public ManagedListView() : base() {
            Init();
        }

        public ManagedListView(IList itemsSource, int itemHeight, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem) : base(itemsSource, itemHeight, makeItem, bindItem) {
            Init();
        }
        private void AddElement() {
            if (addItem != null) {
                addItem.Invoke();
                Refresh();
            }
        }
        private void RemoveElement() {
            if (addItem != null) {
                removeItem.Invoke();
                Refresh();
            }
        }
        private void Init() {
            scrollView = this.Q<ScrollView>(null, "unity-scroll-view");
            commands = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Uxml).CloneTree();
            commands.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Uss));
            commands.AddToClassList(CommandsClassName);
            addButton = commands.Q<Button>(null, AddCommandClassName);
            addButton.clicked += AddElement;
            addButton.style.backgroundImage = Background.FromTexture2D((Texture2D)EditorGUIUtility.IconContent("icons/d_Toolbar Plus.png").image);
            removeButton = commands.Q<Button>(null, RemoveCommandClassName);
            removeButton.style.backgroundImage = Background.FromTexture2D((Texture2D)EditorGUIUtility.IconContent("icons/d_Toolbar Minus.png").image);
            removeButton.clicked += RemoveElement;
            this.RegisterCallback<AttachToPanelEvent>(evt =>
            {
                var self = (ManagedListView)evt.target;
                var parent = self.parent;
                if (parent != null) {
                    var index = parent.IndexOf(self);
                    parent.Insert(index + 1, commands);
                }
            });
            this.RegisterCallback<DetachFromPanelEvent>(evt => commands.RemoveFromHierarchy());
            RefreshButtons();
        }
        private void RefreshButtons() {
            if (removeItem == null) {
                removeButton.AddToClassList(HiddenClassName);
            }
            else {
                removeButton.RemoveFromClassList(HiddenClassName);
            }
            if (addItem == null) {
                addButton.AddToClassList(HiddenClassName);
            }
            else {
                addButton.RemoveFromClassList(HiddenClassName);
            }
            if (addItem == null && removeItem == null) {
                commands.AddToClassList(HiddenClassName);
            }
            else {
                commands.RemoveFromClassList(HiddenClassName);
            }
        }
        public new class UxmlFactory : UxmlFactory<ManagedListView, UxmlTraits> { }
        public new class UxmlTraits : ListView.UxmlTraits { }
    }
}