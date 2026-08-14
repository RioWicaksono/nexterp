"use client";

import { Users, UserCheck, Calendar, TrendingUp, RefreshCw, Plus, Building } from "lucide-react";
import { AppShell } from "@/app/components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  employeeNumber: string;
  department?: string;
  position?: string;
}

interface Department {
  id: string;
  name: string;
  code: string;
}

export default function HRMPage() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"overview" | "employees" | "attendance">("overview");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [empRes, deptRes] = await Promise.all([
        api.get<{ items: Employee[] }>("/api/v1/employees"),
        api.get<{ items: Department[] }>("/api/v1/departments"),
      ]);

      if (empRes.success && empRes.data) {
        setEmployees(empRes.data.items || []);
      }
      if (deptRes.success && deptRes.data) {
        setDepartments(deptRes.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load HRM data:", error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AppShell>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">
              Human Resource Management
            </h1>
            <p className="text-slate-500 dark:text-slate-400">
              Manage employees, attendance, and HR operations
            </p>
          </div>
          <div className="flex gap-2">
            <button onClick={loadData} className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl hover:bg-slate-50 dark:hover:bg-slate-700">
              <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} />
              Refresh
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl">
              <Plus className="w-4 h-4" />
              Add Employee
            </button>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            icon={Users}
            label="Total Employees"
            value={employees.length.toString()}
            color="blue"
          />
          <StatCard
            icon={UserCheck}
            label="Departments"
            value={departments.length.toString()}
            color="emerald"
          />
          <StatCard
            icon={Calendar}
            label="On Leave Today"
            value="0"
            color="purple"
          />
          <StatCard
            icon={TrendingUp}
            label="New This Month"
            value="0"
            color="amber"
          />
        </div>

        {/* Tabs */}
        <div className="border-b border-slate-200 dark:border-slate-700">
          <nav className="flex gap-6">
            {[
              { id: "overview", label: "Overview" },
              { id: "employees", label: "Employees" },
              { id: "attendance", label: "Attendance" },
            ].map(tab => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id as typeof activeTab)}
                className={`
                  pb-3 text-sm font-medium border-b-2 transition-colors
                  ${
                    activeTab === tab.id
                      ? "border-blue-600 text-blue-600"
                      : "border-transparent text-slate-500 hover:text-slate-700 dark:hover:text-slate-300"
                  }
                `}
              >
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Content */}
        {loading ? (
          <div className="flex items-center justify-center h-64">
            <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
          </div>
        ) : (
          <>
            {activeTab === "overview" && (
              <div className="grid lg:grid-cols-2 gap-6">
                {/* Departments */}
                <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
                  <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">
                    Departments
                  </h3>
                  {departments.length === 0 ? (
                    <p className="text-slate-500 text-center py-8">No departments found</p>
                  ) : (
                    <div className="space-y-3">
                      {departments.map(dept => (
                        <div
                          key={dept.id}
                          className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl"
                        >
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                              <Users className="w-5 h-5 text-emerald-600" />
                            </div>
                            <div>
                              <p className="font-medium text-slate-800 dark:text-white">{dept.name}</p>
                              <p className="text-sm text-slate-500">{dept.code}</p>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                {/* Employees */}
                <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
                  <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">
                    Recent Employees
                  </h3>
                  {employees.length === 0 ? (
                    <p className="text-slate-500 text-center py-8">No employees found</p>
                  ) : (
                    <div className="space-y-3">
                      {employees.slice(0, 5).map(emp => (
                        <div
                          key={emp.id}
                          className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl"
                        >
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                              <span className="text-sm font-semibold text-blue-600">
                                {emp.firstName?.[0]}{emp.lastName?.[0]}
                              </span>
                            </div>
                            <div>
                              <p className="font-medium text-slate-800 dark:text-white">
                                {emp.firstName} {emp.lastName}
                              </p>
                              <p className="text-sm text-slate-500">{emp.email}</p>
                            </div>
                          </div>
                          <span className="px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400">
                            Active
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            )}

            {activeTab === "employees" && (
              <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-slate-50 dark:bg-slate-700/50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Employee
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Employee #
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Email
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Status
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                      {employees.length === 0 ? (
                        <tr>
                          <td colSpan={4} className="px-6 py-12 text-center text-slate-500">
                            No employees found
                          </td>
                        </tr>
                      ) : (
                        employees.map(emp => (
                          <tr key={emp.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                            <td className="px-6 py-4 whitespace-nowrap">
                              <div className="flex items-center gap-3">
                                <div className="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                                  <span className="text-sm font-semibold text-blue-600">
                                    {emp.firstName?.[0]}{emp.lastName?.[0]}
                                  </span>
                                </div>
                                <span className="font-medium text-slate-800 dark:text-white">
                                  {emp.firstName} {emp.lastName}
                                </span>
                              </div>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-slate-500">
                              {emp.employeeNumber}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-slate-500">
                              {emp.email}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                              <span className="px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400">
                                Active
                              </span>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {activeTab === "attendance" && (
              <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
                <div className="text-center py-12">
                  <Calendar className="w-12 h-12 text-slate-300 mx-auto mb-4" />
                  <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-2">
                    Attendance Management
                  </h3>
                  <p className="text-slate-500 mb-4">
                    Track employee attendance, clock in/out, and leave requests
                  </p>
                  <button className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-medium transition-colors">
                    View Attendance Records
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </AppShell>
  );
}

function StatCard({
  icon: Icon,
  label,
  value,
  color,
}: {
  icon: React.ElementType;
  label: string;
  value: string;
  color: "blue" | "emerald" | "purple" | "amber";
}) {
  const colors = {
    blue: "bg-blue-100 dark:bg-blue-900/30 text-blue-600",
    emerald: "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600",
    purple: "bg-purple-100 dark:bg-purple-900/30 text-purple-600",
    amber: "bg-amber-100 dark:bg-amber-900/30 text-amber-600",
  };

  return (
    <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
      <div className="flex items-center gap-4">
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${colors[color]}`}>
          <Icon className="w-6 h-6" />
        </div>
        <div>
          <p className="text-sm text-slate-500 dark:text-slate-400">{label}</p>
          <p className="text-2xl font-bold text-slate-800 dark:text-white">{value}</p>
        </div>
      </div>
    </div>
  );
}
