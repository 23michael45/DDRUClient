using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OpenCVForUnity.ArucoModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils.Helper;
using System.IO;

public class VirtualCameraCalibration : MonoBehaviour
{
    public int _CurrentIndex = 0;
    public string _BasePath;
    public RenderTextureSaver _RenderTextureSaver;

    // Start is called before the first frame update
    void Start()
    {
        if (_RenderTextureSaver == null)
        {
            _RenderTextureSaver = GetComponent<RenderTextureSaver>();

        }

    }
    string GetPath(string file)
    {
        var s = Path.Combine(Application.dataPath, _BasePath);
        s = string.Format("{0}/{1}", s , file);
       
        return s;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            _RenderTextureSaver.StartTakePhoto(GetPath(string.Format("{0}.png",_CurrentIndex)));
            _CurrentIndex++;

        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Calibrate();
        }
    }


    void Remap(Mat srcImage,int ind)
    {
        Mat mapx = new Mat(srcImage.size(), srcImage.type());
        Mat mapy = new Mat(srcImage.size(), srcImage.type());
 

        ind = ind % 4;

        for (int j = 0; j < srcImage.rows(); j++)
        {
            for (int i = 0; i < srcImage.cols(); i++)
            {
                switch (ind)
                {
                    //图像长宽缩小到原来的一半并居中显示
                    case 0:
                        if (i > srcImage.cols() * 0.25 && i < srcImage.cols() * 0.75 && j > srcImage.rows() * 0.25 && j < srcImage.rows() * 0.75)
                        {
                            mapx.put(j, i, 2 * (i - srcImage.cols() * 0.25) + 0.5 );
                            mapy.put(j, i ,2 * (j - srcImage.rows() * 0.25) + 0.5 );
                        }
                        else
                        {

                            mapx.put(j, i, 0);
                            mapy.put(j, i, 0);
                        }
                        break;

                    //图像左右翻转
                    case 1:

                        mapx.put(j, i, i);
                        mapy.put(j, i, srcImage.rows() - j);
                        break;

                    //图像上下翻转
                    case 2:
                        mapx.put(j, i, srcImage.cols() - i);
                        mapy.put(j, i,j);
                        break;

                    //图像上下左右均翻转
                    case 3:

                        mapx.put(j, i, srcImage.cols() - i);
                        mapy.put(j, i, srcImage.rows() - j);
                        
                        break;

                    default:
                        break;
                }
            }
        }

        ind++;
    }

    Mat GetExtrinsicMatrix(Mat rotation_Matrix,Mat tvecsMat)
    {

        Mat extrinsicMatrix = new Mat();
        List<Mat> extrinsicInput = new List<Mat>();
        extrinsicInput.Add(rotation_Matrix);
        extrinsicInput.Add(tvecsMat);
        Core.hconcat(extrinsicInput, extrinsicMatrix);

        return extrinsicMatrix;
    }

    Mat GetHomographyMatrix(Mat projectionMatrix)
    {
        double p11 = projectionMatrix.get(0, 0)[0],
                p12 = projectionMatrix.get(0, 1)[0],
                p14 = projectionMatrix.get(0, 3)[0],
                p21 = projectionMatrix.get(1, 0)[0],
                p22 = projectionMatrix.get(1, 1)[0],
                p24 = projectionMatrix.get(1, 3)[0],
                p31 = projectionMatrix.get(2, 0)[0],
                p32 = projectionMatrix.get(2, 1)[0],
                p34 = projectionMatrix.get(2, 3)[0];

        Mat homographyMatrix = new Mat(3, 3, CvType.CV_32F);

        homographyMatrix.put(0, 0, p11);
        homographyMatrix.put(0, 1, p12);
        homographyMatrix.put(0, 2, p14);
        homographyMatrix.put(1, 0, p21);
        homographyMatrix.put(1, 1, p22);
        homographyMatrix.put(1, 2, p24);
        homographyMatrix.put(2, 0, p31);
        homographyMatrix.put(2, 1, p32);
        homographyMatrix.put(2, 2, p34);

        return homographyMatrix;
    }



    bool Calibrate()
    {
        int totalNum = 10;

        string path = GetPath("caliberation_result.txt");

        // Delete the file if it exists.
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        FileStream fsout = File.Create(path);   //保存标定结果的文件
        StreamWriter fout = new StreamWriter(fsout);

        List<string> imgList = new List<string>();
        for(int i = 0; i< totalNum; i++)
        {
            imgList.Add(GetPath(string.Format("{0}.png", i)));

        }
        
        

        //to do load img


        Debug.Log("开始提取角点......");


        Size image_size = new Size();//保存图片大小
        Size pattern_size = new Size(4, 6);//标定板上每行、每列的角点数；测试图片中的标定板上内角点数为4*6
        //vector<Point2f>::iterator corner_points_buf_ptr;
        List<Mat> corner_points_of_all_imgs = new List<Mat>();


        int image_num = 0;
        string filename;

        List<Mat> InputImages = new List<Mat>();

        while (image_num < imgList.Count)
        {
            filename = imgList[image_num++];

            Mat imageInput = Imgcodecs.imread(filename);
            InputImages.Add(imageInput);


            if (image_num == 1)
            {
                image_size.width = imageInput.cols();
                image_size.height = imageInput.rows();
            }
            MatOfPoint2f corner_points_buf = new MatOfPoint2f();//建一个数组缓存检测到的角点，通常采用Point2f形式

            if (!Calib3d.findChessboardCorners(imageInput, pattern_size, corner_points_buf))
            {
                Debug.Log("can not find chessboard corners!\n");   //找不到角点
                return false;;
            }
            else
            {
                Mat gray = new Mat();
                Imgproc.cvtColor(imageInput, gray, Imgproc.COLOR_RGB2GRAY);


                Imgproc.cornerSubPix(gray, corner_points_buf, new Size(5, 5), new Size(-1, -1), new TermCriteria(TermCriteria.EPS + TermCriteria.COUNT, 30, 0.1));
                corner_points_of_all_imgs.Add(corner_points_buf);

                string scorner_points_buf = corner_points_buf.dump();

                Calib3d.drawChessboardCorners(gray, pattern_size, corner_points_buf, true);


                //string imageFileName = filename.Replace(".png", "_corner.png");
                //Imgcodecs.imwrite(imageFileName, gray);

            }
        }

        int total = corner_points_of_all_imgs.Count;
        int cornerNum = (int)(pattern_size.width * pattern_size.height);//每张图片上的总的角点数
        
        Size square_size = new Size(10, 10);  /* 实际测量得到的标定板上每个棋盘格的大小 */


        Mat cameraMatrix = new Mat(3, 3, CvType.CV_32FC1, new Scalar(0));//内外参矩阵，H——单应性矩阵
        MatOfDouble distCoefficients = new MatOfDouble(1, 5, CvType.CV_32FC1);//摄像机的5个畸变系数：k1,k2,p1,p2,k3
        List<Mat> tvecsMat = new List<Mat>();//每幅图像的平移向量，t
        List<Mat> rvecsMat = new List<Mat>();//每幅图像的旋转向量（罗德里格旋转向量）
        List<Mat> objectPoints = new List<Mat>();//保存所有图片的角点的三维坐标
                      

        for (int k = 0; k < image_num; k++)//遍历每一张图片
        {
            MatOfPoint3f tempCornerPoints = new MatOfPoint3f();//每一幅图片对应的角点数组

            List<Point3> pt3arr = new List<Point3>();
            //遍历所有的角点
            for (int i = 0; i < pattern_size.height; i++)
            {
                for (int j = 0; j < pattern_size.width; j++)
                {
                    Point3 singleRealPoint = new Point3();//一个角点的坐标
                    singleRealPoint.x = i * square_size.width;
                    singleRealPoint.y = j * square_size.height;
                    singleRealPoint.z = 0;//假设z=0
                    pt3arr.Add(singleRealPoint);
                }
            }
            tempCornerPoints.fromList(pt3arr);

            objectPoints.Add(tempCornerPoints);
        }


        string s = corner_points_of_all_imgs[0].dump();

        Calib3d.calibrateCamera(objectPoints, corner_points_of_all_imgs, image_size, cameraMatrix, distCoefficients, rvecsMat, tvecsMat, 0);



        Debug.Log("标定完成");

        //开始保存标定结果

        fout.WriteLine("相机相关参数：");
        
        fout.WriteLine("1.内外参数矩阵:");
        fout.WriteLine("大小：" + cameraMatrix.size());
        fout.WriteLine(cameraMatrix.dump());
        
        fout.WriteLine("2.畸变系数：");
        fout.WriteLine("大小：" + distCoefficients.size());
        fout.WriteLine(distCoefficients.dump());
        
        fout.WriteLine("图像相关参数：");

        List<Mat> rotMatList = new List<Mat>();
        for (int i = 0; i < image_num; i++)
        {
            fout.WriteLine("第" + i + "幅图像的旋转向量：");
            fout.WriteLine(rvecsMat[i].dump());
            fout.WriteLine("第" + i + "幅图像的旋转矩阵：");


            Mat rotation_Matrix = new Mat(3, 3, CvType.CV_32FC1, new Scalar(0));//旋转矩阵
            Calib3d.Rodrigues(rvecsMat[i], rotation_Matrix);//将旋转向量转换为相对应的旋转矩阵
            rotMatList.Add(rotation_Matrix);

            fout.WriteLine(rotation_Matrix.dump());
            fout.WriteLine("第" + i + "幅图像的平移向量：");
            fout.WriteLine(tvecsMat[i].dump());


            Mat extrinsicMatrix = GetExtrinsicMatrix(rotation_Matrix, tvecsMat[i]);
            Mat projectionMatrix = cameraMatrix * extrinsicMatrix;
            Mat homographyMatrix = GetHomographyMatrix(projectionMatrix);
            Mat inverseHomographyMatrix = homographyMatrix.inv();


            fout.WriteLine("第" + i + "幅图像的外参矩阵：");
            fout.WriteLine(extrinsicMatrix.dump());

            fout.WriteLine("第" + i + "幅图像的投影矩阵：");
            fout.WriteLine(projectionMatrix.dump());
            
            fout.WriteLine("第" + i + "幅图像的单应矩阵：");
            fout.WriteLine(homographyMatrix.dump());
            


            fout.WriteLine("第" + i + "幅图像的单应逆矩阵：");
            fout.WriteLine(inverseHomographyMatrix.dump());

            Mat input = InputImages[i];
            
            Mat dst = RemapProjection(input, homographyMatrix,cameraMatrix);

            //string imageFileName = imgList[i].Replace(".png", "_rd.png");
            //Imgcodecs.imwrite(imageFileName, dst);

            break;
        }

        Debug.Log("结果保存完毕");

        //对标定结果进行评价
        Debug.Log("开始评价标定结果......");

        //计算每幅图像中的角点数量，假设全部角点都检测到了
        int corner_points_counts;
        corner_points_counts = (int)(pattern_size.width * pattern_size.height);

        Debug.Log("每幅图像的标定误差：");
        fout.WriteLine("每幅图像的标定误差：");
        double err = 0;//单张图像的误差
        double total_err = 0;//所有图像的平均误差
        for (int i = 0; i < image_num; i++)
        {
            MatOfPoint2f image_points_calculated = new MatOfPoint2f();//存放新计算出的投影点的坐标
            MatOfPoint3f tempPointSet =(MatOfPoint3f)objectPoints[i];
            Calib3d.projectPoints(tempPointSet, rvecsMat[i], tvecsMat[i], cameraMatrix, distCoefficients, image_points_calculated);

            //计算新的投影点与旧的投影点之间的误差
            MatOfPoint2f image_points_old = (MatOfPoint2f)corner_points_of_all_imgs[i];
            //将两组数据换成Mat格式
            Mat image_points_calculated_mat = new Mat(1, image_points_calculated.cols(), CvType.CV_32FC2);
            Mat image_points_old_mat = new Mat(1, image_points_old.cols(), CvType.CV_32FC2);
            for (int j = 0; j < tempPointSet.cols(); j++)
            {
                image_points_calculated_mat.put(0, j , image_points_calculated.get(0,j));
                image_points_old_mat.put(0, j, image_points_old.get(0,j));
            }
            err = Core.norm(image_points_calculated_mat, image_points_old_mat, Core.NORM_L2);
            err /= corner_points_counts;
            total_err += err;
            Debug.Log("第" + i + 1 + "幅图像的平均误差：" + err + "像素");
            fout.WriteLine("第" + i + 1 + "幅图像的平均误差：" + err + "像素");
        }
        Debug.Log("总体平均误差：" + total_err / image_num + "像素");
        fout.WriteLine("总体平均误差：" + total_err / image_num + "像素");
        Debug.Log("评价完成");

        fout.Close();
        fsout.Close();

        Mat mapx = new Mat(image_size, CvType.CV_32FC1);
        Mat mapy = new Mat(image_size, CvType.CV_32FC1);

        
        Mat R = Mat.eye(3, 3, CvType.CV_32F);

        Debug.Log("保存矫正图像");

        for (int i = 0; i < image_num; i++)
        {
            Debug.Log("Frame #" + (i + 1));
            

            Calib3d.initUndistortRectifyMap(cameraMatrix, distCoefficients, R, cameraMatrix, image_size, CvType.CV_32FC1, mapx, mapy);

            Mat src_image = Imgcodecs.imread(imgList[i], 1);
            Mat new_image = src_image.clone();
            Imgproc.remap(src_image, new_image, mapx, mapy, Imgproc.INTER_LINEAR);



            //string imageFileName = imgList[i].Replace(".png", "_d.png");
            //Imgcodecs.imwrite(imageFileName, new_image);




        }
        Debug.Log("保存结束");


        return true;
    }


    Mat RemapProjection(Mat srcImage,Mat homographyMatrix, Mat cameraMatrix)
    {
        Mat dstImage = new Mat(srcImage.size(), srcImage.type());

        Mat mapx = new Mat(srcImage.size(), CvType.CV_32FC1);
        Mat mapy = new Mat(srcImage.size(), CvType.CV_32FC1);



        for (int j = 0; j < srcImage.rows(); j++)
        {
            for (int i = 0; i < srcImage.cols(); i++)
            {
                if(i == 255 && j==255)
                {
                    Debug.Log("pause");
                }
                if (i == 375 && j == 151)
                {
                    Debug.Log("pause");
                }

                Mat point2D = new Mat(3, 1, CvType.CV_32FC1);
                point2D.put(0, 0, i);
                point2D.put(1, 0, j);
                point2D.put(2, 0, 1);
                Mat point3D = homographyMatrix.inv() * point2D;

                //Debug.Log("point3D.dump");
                //Debug.Log(point3D.dump());

                Mat cameraMatrix32F = new Mat();
                cameraMatrix.convertTo(cameraMatrix32F, CvType.CV_32FC1);


                double d3x = point3D.get(0, 0)[0];
                double d3y = point3D.get(1, 0)[0];
                double d3z = point3D.get(2, 0)[0];


                Mat eye = Mat.eye(3,3,CvType.CV_32FC1);
                Mat zero = Mat.zeros(3, 1,CvType.CV_32FC1);
                Mat extrinsicMatrix = GetExtrinsicMatrix(eye, zero);
                Mat projectionMatrix = cameraMatrix32F * extrinsicMatrix;


                Mat point3D_4 = new Mat(4, 1, CvType.CV_32FC1);
                point3D_4.put(0, 0, d3x / d3z);
                point3D_4.put(1, 0, d3y / d3z);
                point3D_4.put(2, 0, 0);
                point3D_4.put(3, 0, 1);
                Mat point2D_dst = projectionMatrix * point3D_4;


                //Debug.Log("point2D_dst.dump");
                //Debug.Log(point2D_dst.dump());


                double dx = point2D_dst.get(0, 0)[0];
                double dy = point2D_dst.get(1, 0)[0];
                double dz = point2D_dst.get(2, 0)[0];
                int x = (int)(dx / 100 );
                int y = (int)(dy / 100);

                mapx.put(j, i, x);
                mapy.put(j, i, y );


            }
        }

        Imgproc.remap(srcImage, dstImage, mapx, mapy, Imgproc.INTER_LINEAR);
        return dstImage;
    }
}
