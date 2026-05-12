namespace Botany.Interfaces;

internal interface IRotatable
{
    /// <summary>
    /// The rotation of the object about the Z-axis, in degrees.
    /// </summary>
    float Rotation { get; set; }
}
