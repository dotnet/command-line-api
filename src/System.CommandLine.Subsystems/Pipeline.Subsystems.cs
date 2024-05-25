// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.CommandLine.Subsystems;

namespace System.CommandLine;

public partial class Pipeline
{
    private class Subsystems : IEnumerable<CliSubsystem>
    {
        internal List<CliSubsystem> subsystemList = [];
        private bool dirty;

        internal void Add(CliSubsystem? subsystem, bool insertAtStart = false)
        {
            if (subsystem is not null)
            {
                // TODO: Determine whether to remove and readd. This affects the position in the list
                //if (subsystemList.Contains(subsystem))
                //{
                //    subsystemList.Remove(subsystem);
                //}
                subsystemList.Add(subsystem);
                dirty = true;
            }
        }

        internal void Insert(CliSubsystem? subsystem, CliSubsystem existingSubsystem, bool insertBefore = false)
        {
            if (subsystem is not null)
            {
                if (existingSubsystem.Phase != subsystem.Phase)
                {
                    throw new InvalidOperationException("Subsystems can only be inserted relative to other subsystems in the same phase");
                }
                if (subsystemList.Contains(subsystem))
                {
                    // TODO: Exception or last wins? Same for Add above
                    throw new InvalidOperationException("Subsystems can only be inserted if it had not already been added");
                }

                var existing = subsystemList.IndexOf(existingSubsystem);
                if (existing != -1)
                {
                    throw new InvalidOperationException("Subsystems can only be added relative to subsystems that have previously been added");
                }

                var insertAt = insertBefore ? existing + 1 : existing;
                subsystemList.Insert(insertAt, subsystem);
                dirty = true;
            }
        }

        public IEnumerator<CliSubsystem> GetEnumerator()
        {
            return subsystemList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return subsystemList.GetEnumerator();
        }

        internal IEnumerable<CliSubsystem> EarlyReturnSubsystems
            => subsystemList.Where(x => x.Phase == SubsystemPhase.EarlyReturn).ToList();

        internal IEnumerable<CliSubsystem> ValidationSubsystems
            => subsystemList.Where(x => x.Phase == SubsystemPhase.Validate).ToList();

        internal IEnumerable<CliSubsystem> ExecutionSubsystems
            => subsystemList.Where(x => x.Phase == SubsystemPhase.Execute).ToList();

        internal IEnumerable<CliSubsystem> FinishSubsystems
            => subsystemList.Where(x => x.Phase == SubsystemPhase.Finish).ToList();

    }
}
