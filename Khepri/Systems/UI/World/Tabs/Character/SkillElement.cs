using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Khepri.Entities;
using Khepri.Entities.Components.Skills;

namespace Khepri.UI.World.Tabs.Character
{
    public partial class SkillElement : VBoxContainer
    {
        [ExportGroup("Nodes")]
        [Export] private Godot.Collections.Dictionary<SkillCategory, NodePath> _categoryContainerPaths = null!;

        /// <summary> The category containers resolved from <see cref="_categoryContainerPaths"/>, built once when the scene is ready. </summary>
        private readonly Dictionary<SkillCategory, VBoxContainer> _categoryContainers = new Dictionary<SkillCategory, VBoxContainer>();

        private readonly Dictionary<SkillKind, RichTextLabel> _skillLabels = new Dictionary<SkillKind, RichTextLabel>();


        /// <summary> Resolve the exported node paths into real container references the panel can index by category. </summary>
        public override void _Ready()
        {
            foreach (KeyValuePair<SkillCategory, NodePath> entry in _categoryContainerPaths)
            {
                _categoryContainers.Add(entry.Key, GetNode<VBoxContainer>(entry.Value));
            }
        }


        /// <summary> Force the display panel to reflect the current game's state. </summary>
        /// <param name="selectedEntity"> The entity whose character sheet is shown; when null the sheet is left blank. </param>
        public void ForceUpdate(Entity selectedEntity)
        {
            SkillComponent? skills = selectedEntity.GetComponent<SkillComponent>();
            Boolean isVisible = skills != null && skills.Skills.Count > 0;
            Visible = isVisible;

            if (isVisible)
            {
                // Consider every label dirty.
                Dictionary<SkillKind, RichTextLabel> dirtySkillLabels = new Dictionary<SkillKind, RichTextLabel>(_skillLabels);
                _skillLabels.Clear();

                foreach (SkillCategory category in Enum.GetValues<SkillCategory>())
                {
                    if (_categoryContainers.TryGetValue(category, out VBoxContainer? container))
                    {
                        foreach (EntitySkill skill in skills!.Skills.Where(s => s.Kind.Category == category))
                        {
                            dirtySkillLabels.TryGetValue(skill.Kind, out RichTextLabel? label);
                            if (label == null)  // If we don't find one, we need to create one
                            {
                                label = new RichTextLabel();
                                container.AddChild(label);
                            }
                            else                // If we have, it's not dirty so remove it.
                            {
                                dirtySkillLabels.Remove(skill.Kind);
                            }

                            SetSkillRow(label, skill);
                            _skillLabels.Add(skill.Kind, label);    // Add the verified label back into the clean collection.
                        }
                    }
                }

                // Clean up the dirty labels that are now left over.
                foreach (KeyValuePair<SkillKind, RichTextLabel> dirty in dirtySkillLabels)
                {
                    dirty.Value.QueueFree();
                }
            }
        }


        /// <summary> Sets the label text for a single skill: its name, then its practical rating with the theoretical rating in parentheses. </summary>
        /// <param name="label"> The label to modify. </param>
        /// <param name="skill"> The skill to render. </param>
        private static void SetSkillRow(RichTextLabel label, EntitySkill skill)
        {
            Int32 maximum = (Int32)EntitySkill.MaxLevel;
            String practical = Jaypen.Utilities.Extensions.StringExtensions.BuildDots(skill.PracticalDots, maximum);
            String theoretical = Jaypen.Utilities.Extensions.StringExtensions.BuildDots(skill.TheoreticalDots, maximum);

            label.Text = $"{skill.Kind.Noun}   {practical}  ({theoretical})";
            label.TooltipText = skill.Kind.Summary;
            label.FitContent = true;
        }
    }
}
