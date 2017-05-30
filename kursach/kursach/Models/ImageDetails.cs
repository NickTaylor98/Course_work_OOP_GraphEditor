using System;
public class ImageDetails
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
    public string Extension { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public bool Equals(ImageDetails other)
    {
        return this.Name == other.Name &&
            this.Description == other.Description &&
            this.Path == other.Path &&
            this.FileName == other.FileName &&
            this.Extension == other.Extension &&
            this.Height == other.Height &&
            this.Width == other.Width;
    }
}
