### Importance Sampling according to **Rayleigh phase function** PDF

| ![](https://raw.githubusercontent.com/reinsteam/Unity-PhaseSampling/master/Images/RayleighPhase_12.png) |![](https://raw.githubusercontent.com/reinsteam/Unity-PhaseSampling/master/Images/RayleighPhase_22.png) |
-|-

### Screenshots

- Figures on images in the **left column** has two times fewer samples than the ones in the **right column**
- Figures in **bottom rows** of all images are generated using [**Spherical Fibonacci**](http://lgdv.cs.fau.de/uploads/publications/spherical_fibonacci_mapping_opt.pdf) point set.
- Figures in **upper rows** are generated using completely random number (`Random.Range`).

#### Side by side comparison of samples location

- Figures in **right columns** are generated according to *Rayleigh PDF*. Bluish colour of samples means that sample deviates more from the corresponding sample (green one) generated using uniform distribution.

| ![](https://raw.githubusercontent.com/reinsteam/Unity-PhaseSampling/master/Images/RayleighSplitComparison.png) |![](https://raw.githubusercontent.com/reinsteam/Unity-PhaseSampling/master/Images/RayleighSplitComparison_2.png) |
-|-

#### Overlayed comparison of samples location

- Illustrates connections between samples generated using *Uniform distributing* and samples generated according to *Rayleigh phase function PDF*.

![](https://raw.githubusercontent.com/reinsteam/Unity-PhaseSampling/master/Images/RayleighJoinedComparison.png) | ![](https://raw.githubusercontent.com/reinsteam/Unity-PhaseSampling/master/Images/RayleighJoinedComparison_2.png)
-|-
