
#include <stdio.h>
#include <math.h>     // For sqrt, atan2, fabs
#include <stdlib.h>   // For qsort
#include <float.h>    // For DBL_MAX
#include <stdbool.h>  // For bool type (C99 onwards)
#include <string.h>   // For memcpy (optional, but good practice)

#define MAX 100

// Define MIN macro (be careful with side effects if arguments have them)
#define MIN(a, b) (((a) < (b)) ? (a) : (b))

// Structure for a point (Diem)
struct Diem {
    double x, y;
};

// Global variable to hold the centroid for angle comparison (workaround for qsort context)
static struct Diem g_tam;

// Calculate angle from centroid to point a
double tinhGoc(struct Diem a, struct Diem tam) {
    return atan2(a.y - tam.y, a.x - tam.x);
}

// Comparison function for sorting points by angle around g_tam (for qsort)
int compareGoc(const void *a, const void *b) {
    struct Diem p1 = *(const struct Diem *)a;
    struct Diem p2 = *(const struct Diem *)b;
    double goc1 = tinhGoc(p1, g_tam);
    double goc2 = tinhGoc(p2, g_tam);
    if (goc1 < goc2) return -1;
    if (goc1 > goc2) return 1;
    return 0;
}

// Calculate polygon area using Shoelace formula after sorting vertices by angle
double tinhDienTich(struct Diem a[], int n) {
    if (n < 3) return 0.0; // Area is 0 for less than 3 points

    // Calculate centroid (average of coordinates)
    g_tam.x = 0;
    g_tam.y = 0;
    for (int i = 0; i < n; i++) {
        g_tam.x += a[i].x;
        g_tam.y += a[i].y;
    }
    g_tam.x /= n;
    g_tam.y /= n;

    // Sort vertices by angle using qsort and the global centroid g_tam
    qsort(a, n, sizeof(struct Diem), compareGoc);

    // Calculate area using Shoelace formula on sorted vertices
    double dientich = 0;
    for (int i = 0; i < n; i++) {
        int j = (i + 1) % n; // Next vertex index, wraps around
        dientich += (a[i].x * a[j].y - a[j].x * a[i].y);
    }

    return fabs(dientich) / 2.0;
}

// Calculate distance between two points
double khoangCach(struct Diem a, struct Diem b) {
    double dx = a.x - b.x;
    double dy = a.y - b.y;
    return sqrt(dx * dx + dy * dy);
}

// Comparison function for sorting points by X-coordinate (for qsort)
int compareX(const void *a, const void *b) {
    struct Diem p1 = *(const struct Diem *)a;
    struct Diem p2 = *(const struct Diem *)b;
    if (p1.x < p2.x) return -1;
    if (p1.x > p2.x) return 1;
    return 0;
}

// Comparison function for sorting points by Y-coordinate (for qsort)
int compareY(const void *a, const void *b) {
    struct Diem p1 = *(const struct Diem *)a;
    struct Diem p2 = *(const struct Diem *)b;
    if (p1.y < p2.y) return -1;
    if (p1.y > p2.y) return 1;
    return 0;
}

// Find the closest pair of points using Divide and Conquer
// Note: Requires the input array 'a' to be sorted by X-coordinate beforehand
double timGanNhatRecursive(struct Diem a[], int l, int r) {
    // Base case: If few points, use brute force
    if (r - l <= 3) {
        double minKC = DBL_MAX;
        for (int i = l; i < r; i++) {
            for (int j = i + 1; j < r; j++) {
                minKC = MIN(minKC, khoangCach(a[i], a[j]));
            }
        }
        return minKC;
    }

    // Find the middle point
    int mid = l + (r - l) / 2; // More robust way to calculate mid
    struct Diem midPoint = a[mid];

    // Recursively find the smallest distance in left and right subarrays
    double trai = timGanNhatRecursive(a, l, mid);
    double phai = timGanNhatRecursive(a, mid, r);
    double minKC = MIN(trai, phai);

    // Build an array 'strip' containing points close to the middle vertical line
    struct Diem strip[MAX]; // Assuming MAX is large enough for the strip
    int stripSize = 0;
    for (int i = l; i < r; i++) {
        if (fabs(a[i].x - midPoint.x) < minKC) {
            // Ensure we don't exceed strip array bounds
            if (stripSize < MAX) {
                 strip[stripSize++] = a[i];
            } else {
                // Handle error: strip array overflow
                fprintf(stderr, "Warning: Strip array overflow in timGanNhatRecursive\n");
                // Optionally, resize strip or terminate, depending on requirements.
                // For this example, we'll just stop adding points.
            }
        }
    }

    // Sort the strip points by Y-coordinate
    qsort(strip, stripSize, sizeof(struct Diem), compareY);

    // Find the closest points in the strip
    // Check only points within minKC distance in Y
    for (int i = 0; i < stripSize; i++) {
        for (int j = i + 1; j < stripSize && (strip[j].y - strip[i].y) < minKC; j++) {
            minKC = MIN(minKC, khoangCach(strip[i], strip[j]));
        }
    }

    return minKC;
}

// Wrapper function to sort points by X before calling the recursive function
double timGanNhat(struct Diem a[], int n) {
    if (n < 2) return DBL_MAX; // Need at least 2 points

    // Sort points based on X coordinate
    qsort(a, n, sizeof(struct Diem), compareX);

    // Call the recursive function
    return timGanNhatRecursive(a, 0, n);
}


int main() {
    int n;
    struct Diem dinh[MAX];  // Array to store input vertices
    struct Diem daGiac[MAX]; // Copy for area calculation (gets sorted by angle)

    printf("Nhap so dinh cua da giac: ");
    if (scanf("%d", &n) != 1 || n < 0 || n > MAX) {
         fprintf(stderr, "Loi: So dinh khong hop le.\n");
         return 1;
    }
     if (n < 3) {
         printf("Can it nhat 3 dinh de tao thanh da giac.\n");
     }


    printf("Nhap toa do cac dinh (x y):\n");
    for (int i = 0; i < n; i++) {
        printf("Dinh %d: ", i + 1);
        // Check scanf return value for robust input
        if (scanf("%lf %lf", &dinh[i].x, &dinh[i].y) != 2) {
             fprintf(stderr, "Loi: Nhap toa do khong hop le.\n");
             return 1;
        }
    }

    // Make a copy for area calculation because tinhDienTich sorts its input
    // Using memcpy is efficient for copying arrays/structs
    memcpy(daGiac, dinh, n * sizeof(struct Diem));
    // Alternative: loop copy
    // for (int i = 0; i < n; i++) daGiac[i] = dinh[i];

    // Calculate Area
    double dientich = tinhDienTich(daGiac, n); // daGiac array gets sorted by angle inside

    // Calculate Closest Pair Distance
    // timGanNhat will sort the 'dinh' array by X-coordinate
    double khoangCachNganNhat = timGanNhat(dinh, n);

    printf("\nDien tich da giac: %.4lf\n", dientich); // Format output

    if (n >= 2) {
         printf("Khoang cach 2 dinh gan nhat: %.4lf\n", khoangCachNganNhat);
    } else {
         printf("Khong the tinh khoang cach (can it nhat 2 dinh).\n");
    }


    return 0;
}
