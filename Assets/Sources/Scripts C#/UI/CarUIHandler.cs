using UnityEngine;
using UnityEngine.UI;

public class CarUIHandler : MonoBehaviour
{
    [Header("Car details")]
    public Image carImage;

    private Animator animator = null;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetUpCar(CarData carData)
    {
        if (carData == null || carData.CarColorSchemes == null || carData.CarColorSchemes.Length == 0)
            return;

        // Берем ColorSprite из первой цветовой схемы
        carImage.sprite = carData.CarColorSchemes[0].ColorSprite;
    }

    public void ChangeCarSprite(int colorIndex, CarData carData)
    {
        if (carData == null || carData.CarColorSchemes == null)
            return;

        if (colorIndex < 0 || colorIndex >= carData.CarColorSchemes.Length)
            return;

        // Берем ColorSprite по выбранному индексу
        carImage.sprite = carData.CarColorSchemes[colorIndex].ColorSprite;
    }

    public void StartCarEnteranceAnimation(bool isAppearingOnRightSide)
    {
        if (animator == null) return;

        if (isAppearingOnRightSide)
            animator.Play("CarUIAppearFromRight");
        else
            animator.Play("CarUIAppearFromLeft");
    }

    public void StartCarExitAnimation(bool isExitingOnRightSide)
    {
        if (animator == null) return;

        if (isExitingOnRightSide)
            animator.Play("CarUIDisappearToRight");
        else
            animator.Play("CarUIDisappearToLeft");
    }

    // Animation Events
    public void OnCarExitAnimationCompleated()
    {
        Destroy(gameObject);
    }
}