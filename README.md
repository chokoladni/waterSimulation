# Water & Boat Physics Simulation
A simulation of ambient ocean waves based on IFFT of the Phillips spectrum, accompanied by boat physics simulation and interaction waves caused by the water-boat interaction.

## Ambient waves
Ambient waves are simulated by performing inverse fast fourier transform (IFFT) on the Phillips spectrum, an empirically-obtained statistical model which characterizes waves caused by wind on the open ocean.
![image](https://github.com/chokoladni/waterSimulation/assets/19283862/6c7effd1-ce76-4f6a-9880-c4201d54ebc5)

## Boat physics
Forces affecting a body in water can be divided into hydrostatic and hydrodynamic forces. Static forces are always present, while dynamic forces are caused by the motion of the body relative to the water. Forces that arise from the interaction between the body and the water are very complicated in reality, many of which still haven't been analytically determined, but rather obtained through regression of empirical research results. Forces simulated in this project are buoyancy, skin friction drag force, pressure drag force and slamming force.

### Buoyancy
A hydrostatic force that acts on an object partially or fully submerged in a fluid. The buoyant force is caused by the action of hydrostatic pressure
$$𝑝_ℎ=𝜌𝑔ℎ$$
on a surface:
$$\vec{dF_u} = -p_h\vec{n}dS$$
The horizontal forces are cancelled out, leaving only the vertical forces which act on the horizontal surfaces of the submerged object. Since the hydrostatic pressure is greater on the lower (deeper) surface, the object is pushed upwards. The total force amount is calculated by integrating across the submerged area:
$$F_u = \oint 𝜌𝑔ℎ dS$$

### Skin Friction Drag Force
A hydrodynamic force that is a result of relative motion of the object and fluid: 
$$F_d = \frac{1}{2}\rho C_f Su^2$$
where $\rho$ is the fluid density, $C_f$ the skin drag coefficient, $u$ speed of the fluid relative to the object and $S$ the effective area of the object, defined as an orthographic projection of the object in the direction of movement. The drag coefficient $C_f$ is not constant and depends on the shape of the object, as well as the Reynolds number, which in turn depends on the relative speed of the object. $C_f$ is empirically determined, since an analytical solution doesn't exist. However, a good approximation can be calculated as follows:
$$C_f = \frac{0.075}{(log_10 R_n - 2)^2}$$

### Pressure Drag Force
An approximative force that serves as a substitute for other, more complicated and computationally intensive forces, such as wave-making resistance, breaking wave resistance, spray resistance etc. The force is calculated for every submerged triangle of the vessel mesh as
$$\vec{F_D} = -(C_{PD1} v + C_{PD2} v^2) S (cos \theta )^{f_p}\vec{n}$$
if the $cos \theta$ is positive, and
$$\vec{F_D} = (C_{PS1} v + C_{PS2} v^2) S (cos \theta )^{f_p}\vec{n}$$
if the $cos \theta$ is negative. $S$ is the area of the triangle, $\theta$ is the angle between triangle's normal $\vec{n}$ and the velocity $\vec{v}$, while $C_{PD1}, C_{PD2}$ and $C_{PS1}, C_{PS2}$ are user-defined constants to allow tuning of the vessel as desired. If the $cos \theta$ is positive, the triangle is moving into the fluid and generating pressure against it, so the generated force acts away from the normal. If the $cos \theta$ is negative, the triangle is moving away from the fluid, which reduces the pressure and results in a suction force that acts in the direction of the normal.

### Slamming Force
This force is approximative as well and serves the purpose of easier tuning of vessel behaviour during a sudden entrance into the fluid, i.e. "slamming" into the fluid. In order to calculate this force, submerged area of each triangle is tracked for two most recent frames: $[A_i^S(t), A_i^S(t-dt)]$ ($^S$ stands for submerged), as well as their velocities: $[v_i(t), v_i(t - dt)]$. The data is used to calculate the displaced volume of the fluid per second by the triangle:
$$dV_i(t) = A_i^S(t)v_i(t)$$
and
$$\Gamma_i(t) = \frac{V_i(t) - V_i(t - dt}{S_i dt}$$
then serves as an equivalent of acceleration: if the triangle is completely submerged in both frames, $\Gamma_i(t)$ will be equal to the acceleration of the triangle's center. If the triangle gets completely submerged in one frame, while being completely out of the fluid in the previous frame, $\Gamma_i(t)$ will represent the acceleration needed to stop the triangle in one frame. The slamming force is calculated as
$$\vec{F_{slam} = clamp\left(\frac{\Gamma_i(t)}{\Gamma_{max}}, 0, 1\right)^p cos \theta \vec{F_{stop}}}$$
$\Gamma_{max}$ and $p$ are user-defined parameters - $\Gamma_{max}$ represents the acceleration at which the whole stopping force $\vec{F_{stop}}$ will be applied, while $p$ is used to achieve non-linearity. The stopping force is defined as
$$\vec{F_{stop}} = m \frac{v_i}{dt} \frac{2 A_i^S(t)}{S_b}$$
where $m$ is the vessel mass and $S_b$ is the total vessel area.
