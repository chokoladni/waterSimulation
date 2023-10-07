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

# Interaction waves
Interaction waves are the result of water-boat interaction and are simulated using the wave particles method.
## Wave particle
A wave particle represents local water surface deformation at its position. Wave particles move only in the horizontal plane and are mutually independent, meaning there's no interaction between them. Every wave particle consists of an origin point, birth point, time of birth, amplitude, orientation, radius, velocity and dispersion angle.
### Vertical deformation
The shape of the local deformation is declared as $D_i(\vec{x}, t)$, where $\vec{x} = (x, y)$ is the position vector, and the global deformation can then be defined as a superposition of all wave particles' local deformations:
$$\delta_z = \sum_{i} D_i(\vec{x}, t).$$
The local deformation function is defined as
$$D_i(\vec{x}, t) = a_i W(\vec{x} - \vec{x_i}(t))$$
where $a_i$ is the particle amplitude, $\vec{x_i}(t)$ its position at time $t$ and $W$ a constant function which defines the shape of all wave particles:
$$W(\vec{x}) = \frac{1}{2} \left(cos\left(\frac{\pi |\vec{x}|}{r}\right) + 1\right)\Pi\left(\frac{|\vec{x}|}{2r}\right)$$
where $r$ is the particle radius.
The following image shows the shape of a single wave particle:
![image](https://github.com/chokoladni/waterSimulation/assets/19283862/69ee6596-d00d-4243-8fc7-63a521db8e73)

### Horizontal deformation
However, real waves are shaped a bit differently - they aren't as smooth as the sinusoidal representation above, but rather have pointy tips. To model this, beside the vertical deformation, a horizontal deformation is introduced as well:
$$\vec{H_i}(\vec{x}, t) = - \frac{a_i \sqrt{2}}{2} sin \left( \frac{\pi |\vec{p}|}{r}\right) \left( cos \left(\frac{\pi |\vec{p}|}{r} \right) +1 \right) \Pi \left( \frac{|\vec{p}|}{2r} \right)\hat{p}$$
where $\vec{p} = \vec{x} - \vec{x_i}(t)$ is shorthand for the vector between particle's position and the given position at which the deformation is being evaluated. The final shape of the deformation, with both the vertical and horizontal components, is depicted below.
![image](https://github.com/chokoladni/waterSimulation/assets/19283862/664541dd-dc2b-48cc-a4ac-92d2cdc18344)

### Wave fronts
A wavefront is modeled by arranging particles next to each other, with the important consideration that the wave particles are close enough to each other for the wavefront to be continuous. The result is an approximation of the wavefront, but the error is practically imperceptible if the distance between particles remains less than half the radius. To ensure this in case of a curved wavefront, particles are split into multiple smaller particles whenever the distance between them would exceed half the radius. The dispersion angle $\phi$ and origin point $O$ are used here to determine when a certain particle needs to split:
![image](https://github.com/chokoladni/waterSimulation/assets/19283862/4c063db2-b2d8-48f7-a0c0-01ba49356430)
When the width $w$ of the wavefront that the particle represents exceeds its radius, the particle will be split into 3 new smaller particles, each of which will inherit the origin point $O$, a third of the dispersion angle $phi$ and a third of the amplitude $a$. One of the new particles will have the birth point equal to the current position of the parent particle, while the parent's position will need to be rotated around the origin point by $\pm\phi/2$ to determine birth point of the other two particles.

## Generating wave particles
Wave particles are generated each frame when the boat is partially or fully submerged into the water, in a few steps: first the silhouette of the submerged part of the boat is calculated, then wave effects of submerged triangles are calculated. The edges of the silhouette and propagation directions need to be calculated, after which the wave effects can be distributed to the edges. Finally, wave particles are generated based on the distributed wave effects.

### Submerged silhouette
The submerged silhouette is generated by orthographically projecting the submerged part of the boat onto a horizontal plane. A custom fragment shader is used to store the depth of each pixel in the red channel, while the green channel is used to store the normalised vertical component of the normal vector of the ship at the given pixel, scaled to range $[0, 1]$.

![image](https://github.com/chokoladni/waterSimulation/assets/19283862/2540351d-ed12-463b-835c-7d912f3cebb9)

The silhouette is kept at a low resolution (32x32 in the image above) for performance reasons, since it will be read on the CPU at the final stage, during particle generation. It is also a requirement for the resolution to be square and a power of 2, for later stages of the process.

### Wave effects
Wave effects are calculated for each submerged triangle:
$$E_i = S_i (\vec{v_i} * \vec{n_i} dt$$
where $S_i$ is the triangle area, $\vec{v_i}$ velocity, $\vec{n_i}$ normal and $dt$ time step between frames. Wave effect represents the volume of water displaced by a moving triangle - it is positive if the triangle is moving into the water and negative if it is moving away from the water (i.e. away from its normal). Wave effect corresponds to the amplitude of a wave particle that will be generated later on - the greater the displaced volume, the greater the amplitude. If the wave effect is negative, resulting particle will have a negative amplitude as well, which will result in a depression.
After the wave effects have been calculated, they need to be written to the silhouette texture. It is important to distinguish between direct and indirect effects. Specifically, if the triangle is located on the upper side of the boat, its influence is direct and waves are generated directly above the triangle. On the other hand, if the triangle is on the bottom side of the boat, it's wave effect is indirect and will need to be propagated to the edge of the silhouette, where the wave will be generated. Sketch below compares the direct (green) and indirect (red) wave effects.

![image](https://github.com/chokoladni/waterSimulation/assets/19283862/f32c0cb7-2614-451f-816b-ece5899612bb)

Wave effects are written into the texture using a compute shader. The vertical normal component from the silhouette is used here to determine whether it was a direct or an indirect effect. If the vertical normal compoent is negative (i.e., green channel < 0.5), the effect will be negative, but a further check against the depth (red channel) is required if the vertical normal component is positive. If the triangle's depth is greater than the written depth, it is determined to be on the bottom part of the boat and its wave effect is considered indirect. On the other hand, if the triangle's depth is (approximately) the same as the written depth, it is considered to be on the top part of the boat and its effect is direct. Direct effects are written into the blue channel, while indirect effects are written into the alpha channel.

### Silhouette edge and propagation
The third step in the wave particle generation process is calculation of the silhouette edges and particle propagation directions. To be precise, the detected pixels are not part of the silhouette, but have at least one neighbouring pixel that is. The silhouette edge is calculated in a compute shader which takes the silhouette texture as an input and outputs the silhouette edge texture of the same dimensions. Each pixel of the input texture is checked - if the pixel is a part of silhouette, zero is written to the output. If the pixel is not a part of the silhouette, its 8 neighbours are checked - if none are a part of the silhouette, zero is the output. However, if any are a part of the silhouette, it is detected as an edge and 1 is written to the blue channel to indicate that. Furthermore, propagation directions are calculated based on those neighbours, as an averaged normalised vector that points away from any neighbours that are part of the silhouette. The components of the vector are written into the red (horizontal component) and green (vertical component) channel. Also, the values from the alpha channel of the silhouette texture (i.e. the indirect wave effects) are copied to the alpha channel of the output texture - these effects will be distributed to the edge of the silhouette in the next step.

### Indirect wave effects distribution
Indirect wave effect distribution is performed in two substeps, and alongside it, the propagation directions are smoothened. First the resolution reduction of the silhouette edge texture with indirect effects is performed in $n$ iterations, where $n=log_2(N)$ and $N$ is the original texture dimension. In each step the resolution is halved in both width and height, so the texture in the last iteration will only be 1x1. Iterations are performed by a compute shader which takes the result of previous iteration as an input, and outputs a resolution of halved dimensions. For each pixel of the output texture the corresponding 4 pixels of the input texture are considered - every channel of the 4 pixels is summed up and written into the corresponding channel of the output pixel (so that all red channel values are summed up and written into the output red channel, same for all greens, blues and alphas). Red and green channels will then contain the sum of propagation directions, blue channel will contain the number of edge pixels and alpha will contain the sum of indirect wave effects. Result of every iteration is saved into a separate texture, as that will be required for the second substep. 

The second substep is to upscale the reduced textures from the previous substep back to the original dimensions (NxN). The compute shader which performs this substep takes as input (1) the output of the previous iteration (in the first iteration that would be the 1x1 texture from the first substep) and (2) a texture from the first substep which has double the dimensions of the first input texture (1). Red and green channels of the output texture are used to store the averaged propagation directions from the input textures. Indirect wave effects are written only to pixels which have blue channel greater than 0, i.e. pixels which are considered to be on the edge of the silhouette. The wave effects are calculated as a sum of the input texture's wave effects divided by the blue channel. The output blue channel is left unused at this point. The final output texture of the last iteration will contain the wave effects distributed to the edges of the silhouette.

After the distribution has been completed, the direct wave effects from the original silhouette texture are copied to the blue channel of the texture with distributed wave effects, which results in the final texture of this step - it contains smoothened (averaged) propagation directions (red and green channel), direct wave effects (blue channel) and distributed indirect wave effects (alpha channel). This texture is used in the final step.

### Wave particle generation
The texture from the previous step is read pixel by pixel on the CPU and for each pixel that contains a wave effect (either direct, indirect, or both), a wave particle is generated. If the wave effect is direct, the generated particle won't have a defined propagation direction and its dispersion angle $\phi$ will be $2\pi$, meaning it will disperse in every direction and form a circular wavefront. If the wave effect is indirect, the generated particle will have the propagation direction that is written in the texture and its dispersion angle will be the average angle between its propagation direction and its neighbours' propagation directions. Amplitudes of the generated particles are read from blue and alpha channels.
