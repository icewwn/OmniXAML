﻿namespace OmniXaml.ReworkPhases
{
    using System;
    using System.Linq;
    using Rework;
    using Zafiro.Core;

    public class MemberAssigmentApplier : IMemberAssigmentApplier
    {
        private readonly IValuePipeline pipeline;

        public MemberAssigmentApplier(IValuePipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        public void ExecuteAssignment(MemberAssignment inflatedAssignment, object instance)
        {
            if (inflatedAssignment.Member.MemberType.IsCollection())
            {
                AssignCollection(inflatedAssignment, instance);
            }
            else
            {
                AssignSingleValue(inflatedAssignment, instance);
            }
        }

        private void AssignSingleValue(MemberAssignment inflatedAssignment, object instance)
        {
            var inflatedAssignmentChildren = inflatedAssignment.Values.ToList();

            if (inflatedAssignmentChildren.Count > 1)
            {
                throw new InvalidOperationException($"Cannot assign multiple values to a the property {inflatedAssignment}");
            }

            var first = inflatedAssignmentChildren.First();
            
            var value = first.Instance;

            SetMember(instance, inflatedAssignment.Member, value);
        }

        private void SetMember(object parent, Member member, object value)
        {
            var mutableUnit = new MutablePipelineUnit(value);
            pipeline.Handle(parent, member, mutableUnit);
            if (mutableUnit.Handled)
            {
                return;
            }

            member.SetValue(parent, mutableUnit.Value);
        }

        private static void AssignCollection(MemberAssignment inflatedAssignment, object instance)
        {
            var parent = inflatedAssignment.Member.GetValue(instance);
            var children = from n in inflatedAssignment.Values select n.Instance;
            foreach (var child in children)
            {
                Collection.UniversalAdd(parent, child);
            }
        }
    }
}