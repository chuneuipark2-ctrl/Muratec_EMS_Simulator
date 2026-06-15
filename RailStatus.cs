namespace EMS_TEST_SIMULATOR
{
    /// <summary>EMS H1 상태 → SR50/SR150 레일 UI 공유.</summary>
    public static class RailStatus
    {
        public static string CurrentSectionCount { get; set; } = "";
        public static string TargetSectionCount { get; set; } = "";
        /// <summary>0=이동(주행), 1=탑재, 2=이재 등</summary>
        public static string TargetActionMode { get; set; } = "";

        public static void SyncFromEmsStatus(SKY_RAV_Status status)
        {
            if (status == null) return;
            CurrentSectionCount = status.CurrentSectionCount ?? "";
            TargetSectionCount = status.TargetSectionCount ?? "";
            TargetActionMode = status.TargetActionMode ?? "";
        }
    }
}
