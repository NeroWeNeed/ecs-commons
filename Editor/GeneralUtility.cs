namespace NeroWeNeed.Commons {
    public static class GeneralUtility {
        public static bool FixDirectory(ref string directory, string defaultDirectory) {
            var dirty = false;
            if (string.IsNullOrWhiteSpace(directory)) {
                directory = defaultDirectory;
                dirty = true;
            }
            if (directory.EndsWith("/")) {
                directory = directory.Substring(0, directory.Length - 1);
                dirty = true;
            }
            return dirty;
        }
        
    }
}