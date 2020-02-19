# Computational Geometry Unity Library

This library consists of two folders. The idea is that one is for testing purposes and the other folder is the folder you drag into your project. 

## Finished


### 1. Test

* Is a triangle oriented clockwise?
* Is a point left, on, or right of vector?
* Is a point left, on, or right of a plane?
* Is a point inside or outside a circle?
* Is a quadrilateral convex?


### 2. Intersection

* Point in triangle
* Point in polygon
* Triangle-triangle
* AABB
* Rectangle intersection with Separating Axis Theorem (SAT)
* Line-line
* Ray-plane


### 3. Convex Hull

* Jarvis March


### 4. Triangulation

* Triangulate convex polygon
* Triangulate convex random points with "visible edge" and "point-by-point" algorithms
* Triangulate with marching squares

#### Delaunay triangulation
* Triangulate convex random points with delaunay triangulation - "point-by-point"
* Triangulate convex random points with delaunay triangulation - "triangulate and then flip all edges"
* Constrained delaunay triangulation


### 5. Voronoi diagram

* Voronoi point-by-point (suffers from floating point precision issues)
* Voronoi from delaunay


## TODO

* Constrained dynamic delaunay triangulation
* Convex hull: Quick Hull from the Valve paper on Hull
* Convex hull: Gift wrapping
* 3d voronoi
* Marching cubes
* Voronoi - incremental 
* Cut 3d mesh with plane
* Metaballs
