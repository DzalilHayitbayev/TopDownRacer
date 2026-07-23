using UnityEngine;
using UnityEngine.UI;

public class CarUIHandler : MonoBehaviour
{
    [Header("Car details")]
    public Image carImage;

    Animator animator = null;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {

    }

    public void SetUpCar(CarData carData)
    {
        carImage.sprite = carData.CarColorSchemes[0];
    }

    public void ChangeCarSprite(int colorIndex, CarData carData)
    {
        if (colorIndex < 0 || colorIndex >= carData.CarColorSchemes.Length)
            return;
        carImage.sprite = carData.CarColorSchemes[colorIndex];
    }

    public void StartCarEnteranceAnimation(bool isAppearingOnRightSide)
    {
        if (isAppearingOnRightSide)
            animator.Play("CarUIAppearFromRight");
        else animator.Play("CarUIAppearFromLeft");

    }

    public void StartCarExitAnimation(bool isExitingOnRightSide)
    {
        if (isExitingOnRightSide)
            animator.Play("CarUIDisappearToRight");
        else animator.Play("CarUIDisappearToLeft");
    }

    //Events
    public void OnCarExitAnimationCompleated()
    {
        Destroy(gameObject);
    }
}
