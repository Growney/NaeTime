using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Events;
public record AnnouncerConfigurationCreated(Guid AnnouncerId, string Name);
public record AnnouncerConfigurationRenamed(Guid AnnouncerId, string NewName);
public record AnnouncerOpenPracticeDetectionAnnouncementEnabled(Guid AnnouncerId);
public record AnnouncerOpenPracticeDetectionAnnouncementDisabled(Guid AnnouncerId);
public record AnnouncerOpenPracticeDetectionPilotNamePrioritySet(Guid AnnouncerId, int priority);
public record AnnouncerOpenPracticeDetectionPilotLatestLapDurationPrioritySet(Guid AnnouncerId, int priority);
public record AnnouncerOpenPracticeDetectionPilotLatestSingleLapDurationPrioritySet(Guid AnnouncerId, int priority);
public record AnnouncerOpenPracticeDetectionPilotLatestConsecutiveLapDurationPrioritySet(Guid AnnouncerId, uint LapCount, int priority);
public record AnnouncerOpenPracticePilotFrequencyAssignedAnnouncementEnabled(Guid AnnouncerId);
public record AnnouncerOpenPracticePilotFrequencyAssignedAnnouncementDisabled(Guid AnnouncerId);

public record AnnouncerTimerConnectionStatusAnnouncementEnabled(Guid AnnouncerId);
public record AnnouncerTimerConnectionStatusAnnouncementDisabled(Guid AnnouncerId);
public record AnnouncerTimerLaneMismatchAnnouncementEnabled(Guid AnnouncerId);
public record AnnouncerTimerLaneMismatchAnnouncementDisabled(Guid AnnouncerId);