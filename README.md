# Computational Geometry Unity Library

This library consists of two folders. The idea is that one is for testing purposes and the other folder is the folder you drag into your project. 

Make sure all input coordinates are normalized to range 0-1 to avoid floating point precision issues! Normalizing methods exists in HelpMethods

## Finished


### 1. Intersection

2d-space:

* Point-triangle

![Intersection point-triangle](/_media/intersections-point-triangle.png?raw=true)

* Point-polygon (suffers from floating point precision issues) 

![Intersection point-polygon](/_media/intersections-point-polygon.png?raw=true)

* Triangle-triangle		

![Intersection triangle-triangle](/_media/intersections-triangle-triangle.png?raw=true)

* AABB-AABB 			

![Intersection point-triangle](/_media/intersections-aabb-aabb.png?raw=true)

* Line-line 	

![Intersection line-line](/_media/intersections-line-line.png?raw=true)
				
* Ray-plane 	

![Intersection ray-plane](/_media/intersections-ray-plane.png?raw=true)
				
* Line-plane 	

![Intersection line-plane](/_media/intersections-line-plane.png?raw=true)
		
* Point-circle 	

![Intersection point-circle](/_media/intersections-point-circle.png?raw=true)			


### 2. Generate mesh

* Grid mesh	


### 3. Convex Hull

* Jarvis March (Gift wrapping)

![Convex hull jarvis march](/_media/convex-hull-jarvis.png?raw=true)	


### 4. Triangulation

* Triangulate convex polygon - is working with colinear points

![Triangulation convex polygon](/_media/triangulation-convex-polygon.png?raw=true)	

* Triangulate points with "visible edge" algorithm - is maybe working with colinear points

![Triangulation visible edges](/_media/triangulation-visible-edges.png?raw=true)	

* Triangulate points with "point-by-point" algorithm - is working with colinear points (because Triangulate convex polygon is working with colinear points)

![Triangulation point-by-point](/_media/triangulation-point-by-point.png?raw=true)	

* Triangulate with marching squares

#### 4.1 Delaunay triangulation

* Delaunay triangulation - "point-by-point" 

![Triangulation Delaunay point-by-point](/_media/triangulation-delaunay-point-by-point.png?raw=true)	

* Delaunay triangulation - "triangulate and then flip edges" 

![Triangulation Delaunay flip edges](/_media/triangulation-delaunay-flip-edges.png?raw=true)	

* Constrained delaunay triangulation 

![Triangulation Delaunay constrained](/_media/triangulation-delaunay-constrained.png?raw=true)	


### 5. Voronoi diagram

* Voronoi from delaunay

![Voronoi from delaunay](/_media/voronoi-from-delaunay.png?raw=true)	



### 6. Polygon clipping

* Greiner-Hormann 

![Polygon clipping greiner](/_media/polygon-clipping-greiner.png?raw=true)	

* Sutherland-Hodgman 

![Polygon clipping sutherland](/_media/polygon-clipping-sutherland.png?raw=true)	


### 7. Other

* Is a triangle oriented clockwise? 

![Triangle orientation](/_media/other-triangle-orientation.png?raw=true)

* Is a point left, on, or right of vector? 

![Point vector orientation](/_media/other-point-vector.png?raw=true)

* Is a point left, on, or right of a plane? Which is the same as the distance to the plane. 

![Point plane orientation](/_media/other-point-plane.png?raw=true)

* Is a quadrilateral convex? 

![Quadrilateral convex or concave](/_media/other-quadrilateral.png?raw=true)

* Is a point between two other points on the same line? 

![Is point between points on line segment](/_media/other-point-between-points.png?raw=true)

* Closest point on a line-segment? 

![Closest point on line segment](/_media/other-closest-point-line-segment.png?raw=true)


## TODO

### Algorithms to implement

* Dynamic constrained delaunay triangulation
* Convex hull: Quickhull from the Valve paper
* Convex hull: Graham scan
* Marching cubes
* Cut 3d mesh with plane
* Metaballs
* Voronoi with Fortune's algorithm
* Triangulation concave polygon by ear clipping
* Rectangle-rectangle with SAT

### Stuff to fix

* The triangulation by splitting triangles, there's a small problem if a vertex ends up on the edge and is not inside the triangle
* Voronoi point-by-point (suffers from floating point precision issues) Because the data was not normalized?
* Point-polygon intersection floating point precision issues


## Socials

Follow me on Twitter for more Unity stuff: https://twitter.com/eriknordeus
