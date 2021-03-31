namespace ValheimTooler.Core.Extensions
{
    public static class MinimapExtensions
    {
        public static void VTExploreAll(this Minimap minimap)
        {
            if (minimap != null)
            {
                minimap.ExploreAll();
            }
        }

        public static void VTReset(this Minimap minimap)
        {
            if (minimap != null)
            {
                minimap.Reset();
            }
        }
    }
}
