
namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(Blackboard))]
    public class BlackboardEditor : Editor
    {
        private HashSet<Component> componentsCache = new();

        protected override void NewOnInspectorGUI()
        {
            // Showing all the blackboard component fields
            this.Foldout("Blackboard Component", () =>
            {
                this.DrawDefaultInspector();
            });

            // Early out if no members are configured
            var blackboard = this.target as Blackboard;

            if (blackboard.MembersToExpose == null)
            {
                return;
            }

            // Drawing all the blackboard elements
            this.componentsCache.Clear();

            foreach (var memeber in blackboard.MembersToExpose)
            {
                if (memeber.component == null || this.componentsCache.Contains(memeber.component))
                {
                    continue;
                }

                this.componentsCache.Add(memeber.component);

                DrawComponent(memeber.component, blackboard.MembersToExpose);
            }

            void DrawComponent(Component component, List<Blackboard.Member> members)
            {
                if (members.Any(x => x.component == component) == false)
                {
                    return;
                }

                this.Foldout($"{component.GetType().Name} Component", () =>
                {
                    GUILayout.Space(10);

                    foreach (var member in members)
                    {
                        if (member.component == component)
                        {
                            if (member.type == Blackboard.MemberType.Member)
                            {
                                this.DrawMember(member.component, member.fieldName);
                            }
                            else if (member.type == Blackboard.MemberType.Property)
                            {
                                this.DrawProperty(member.component, member.fieldName);
                            }
                            else if (member.type == Blackboard.MemberType.SortingLayer)
                            {
                                this.DrawProperty(member.component, member.fieldName);
                            }
                            else
                            {
                                GUILayout.Label($"Unable to find property \"{member.fieldName}\"");
                            }
                        }
                    }

                    GUILayout.Space(10);
                }, true);
            }
        }
    }
}
