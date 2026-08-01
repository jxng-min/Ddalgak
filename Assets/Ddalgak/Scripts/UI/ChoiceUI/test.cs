using Ddalgak;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField]
    private ButtonActionView buttonActionView;

    private void Start()
    {
        buttonActionView.Show(EButtonActionType.Timing, 2f);
    }
}
