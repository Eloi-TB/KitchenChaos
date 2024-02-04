using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class KitchenObjectSO : ScriptableObject
{
    // Els posa en públic porque mai escriu a un Scriptable Object. Però el puc posar en privat i accedir igualment amb una mica més de codi.
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
}
