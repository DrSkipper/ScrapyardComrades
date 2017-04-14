using UnityEngine;

public class SCLight : VoBehavior
{
    public Light Light;
    public Transform LightTransform;

    public void ConfigureLight(NewMapInfo.MapLight lightInfo)
    {
        this.Light.type = (LightType)lightInfo.light_type;
        this.Light.intensity = lightInfo.intensity;
        this.LightTransform.SetLocalZ(-lightInfo.distance);
        this.Light.range = lightInfo.range;
        this.Light.color = new Color(lightInfo.r, lightInfo.g, lightInfo.b, 1.0f);

        if (this.Light.type == LightType.Spot)
            this.Light.spotAngle = lightInfo.spot_angle;

        this.LightTransform.rotation = Quaternion.identity;
        this.LightTransform.Rotate(Vector3.right, lightInfo.rot_x);
        this.LightTransform.Rotate(Vector3.up, lightInfo.rot_y);
    }
}
