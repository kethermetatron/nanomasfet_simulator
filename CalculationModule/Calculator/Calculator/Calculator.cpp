// Calculator.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include "Constants.h"
#include "MOSFETSimulator.h"
#include "RequestReader.h"
#include "RectangularPotentialBarrierModel.h"

RectangularPotentialBarrierModel::RectangularPotentialBarrierModel RectangularPotentialBarrierModel1;
void AddModels()
{
	ModelSelector::Models.push_back(&RectangularPotentialBarrierModel1);
}
int main(int argc, char* argv[])
{
	omp_set_num_threads(4);
	if (argc < 2)
	{
		printf("Commands are Need\n");
		return 0;
	}
	if (argc >= 3)
	{
		AddModels();
		string Command(argv[1]);
		if (Command == "-Simulation")
		{
			string JsonPath(argv[2]);
			MOSFETSimulator::MOSFETSimulation Simulation;
			RequestReader::ReadRequest(JsonPath);
			RequestReader::SetSimulationModel(Simulation);
			RequestReader::SetSimulationParameters(Simulation);
			Simulation.Simulate();
			Simulation.Save();
			Simulation.ShowLeakageCurrent();
			return 0;
		}
		else if (Command == "-DisplayModelDescription")
		{
			string JsonPath(argv[2]);
			ModelSelector::DisplayAllModels(JsonPath);
		}
		else
		{
			printf("Wrong Command");
			return 0;
		}
	}

}

// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
