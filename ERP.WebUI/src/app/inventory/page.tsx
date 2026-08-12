"use client";

import { Package, Warehouse, AlertTriangle } from "lucide-react";
import { AppShell } from "../components/AppShell";
import { api } from "@/lib/api";
import { useEffect, useState } from "react";

interface Warehouse {
  id: string;
  name: string;
  code: string;
  isActive: boolean;
}

interface StockItem {
  id: string;
  itemName: string;
  itemCode: string;
  quantity: number;
  warehouseId: string;
}

export default function InventoryPage() {
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [stockItems, setStockItems] = useState<StockItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<"overview" | "warehouses" | "stock">("overview");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [warehouseRes, stockRes] = await Promise.all([
        api.get<{ items: Warehouse[] }>("/api/v1/warehouses"),
        api.get<{ items: StockItem[] }>("/api/v1/stock-items"),
      ]);

      if (warehouseRes.success && warehouseRes.data) {
        setWarehouses(warehouseRes.data.items || []);
      }
      if (stockRes.success && stockRes.data) {
        setStockItems(stockRes.data.items || []);
      }
    } catch (error) {
      console.error("Failed to load inventory data:", error);
    } finally {
      setLoading(false);
    }
  };

  const totalStock = stockItems.reduce((sum, item) => sum + item.quantity, 0);
  const lowStockItems = stockItems.filter(item => item.quantity < 10);

  return (
    <AppShell>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-white">
              Inventory Management
            </h1>
            <p className="text-slate-500 dark:text-slate-400">
              Manage warehouses, stock items, and inventory levels
            </p>
          </div>
          <button className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl font-medium transition-colors">
            + Add Stock Item
          </button>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard
            icon={Warehouse}
            label="Warehouses"
            value={warehouses.length.toString()}
            color="blue"
          />
          <StatCard
            icon={Package}
            label="Total Stock Items"
            value={stockItems.length.toString()}
            color="emerald"
          />
          <StatCard
            icon={Package}
            label="Total Quantity"
            value={totalStock.toLocaleString()}
            color="purple"
          />
          <StatCard
            icon={AlertTriangle}
            label="Low Stock Items"
            value={lowStockItems.length.toString()}
            color="amber"
          />
        </div>

        {/* Tabs */}
        <div className="border-b border-slate-200 dark:border-slate-700">
          <nav className="flex gap-6">
            {[
              { id: "overview", label: "Overview" },
              { id: "warehouses", label: "Warehouses" },
              { id: "stock", label: "Stock Items" },
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
                {/* Warehouses List */}
                <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
                  <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">
                    Warehouses
                  </h3>
                  {warehouses.length === 0 ? (
                    <p className="text-slate-500 text-center py-8">No warehouses found</p>
                  ) : (
                    <div className="space-y-3">
                      {warehouses.map(wh => (
                        <div
                          key={wh.id}
                          className="flex items-center justify-between p-3 bg-slate-50 dark:bg-slate-700/50 rounded-xl"
                        >
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                              <Warehouse className="w-5 h-5 text-blue-600" />
                            </div>
                            <div>
                              <p className="font-medium text-slate-800 dark:text-white">{wh.name}</p>
                              <p className="text-sm text-slate-500">{wh.code}</p>
                            </div>
                          </div>
                          <span
                            className={`px-2 py-1 rounded-full text-xs font-medium ${
                              wh.isActive
                                ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                                : "bg-slate-100 text-slate-500 dark:bg-slate-700"
                            }`}
                          >
                            {wh.isActive ? "Active" : "Inactive"}
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                {/* Low Stock Alert */}
                <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6">
                  <h3 className="text-lg font-semibold text-slate-800 dark:text-white mb-4">
                    Low Stock Alert
                  </h3>
                  {lowStockItems.length === 0 ? (
                    <p className="text-slate-500 text-center py-8">All items are well stocked! ✅</p>
                  ) : (
                    <div className="space-y-3">
                      {lowStockItems.slice(0, 5).map(item => (
                        <div
                          key={item.id}
                          className="flex items-center justify-between p-3 bg-amber-50 dark:bg-amber-900/20 rounded-xl border border-amber-200 dark:border-amber-800"
                        >
                          <div className="flex items-center gap-3">
                            <AlertTriangle className="w-5 h-5 text-amber-600" />
                            <div>
                              <p className="font-medium text-slate-800 dark:text-white">{item.itemName}</p>
                              <p className="text-sm text-slate-500">{item.itemCode}</p>
                            </div>
                          </div>
                          <span className="px-3 py-1 rounded-full text-sm font-semibold bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
                            {item.quantity} left
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            )}

            {activeTab === "warehouses" && (
              <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {warehouses.map(wh => (
                  <div
                    key={wh.id}
                    className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 p-6"
                  >
                    <div className="flex items-center gap-4 mb-4">
                      <div className="w-12 h-12 rounded-xl bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center">
                        <Warehouse className="w-6 h-6 text-blue-600" />
                      </div>
                      <div>
                        <h4 className="font-semibold text-slate-800 dark:text-white">{wh.name}</h4>
                        <p className="text-sm text-slate-500">{wh.code}</p>
                      </div>
                    </div>
                    <div className="flex items-center justify-between">
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${
                          wh.isActive
                            ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                            : "bg-slate-100 text-slate-500 dark:bg-slate-700"
                        }`}
                      >
                        {wh.isActive ? "Active" : "Inactive"}
                      </span>
                      <button className="text-sm text-blue-600 hover:text-blue-700 font-medium">
                        View Details →
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {activeTab === "stock" && (
              <div className="bg-white dark:bg-slate-800 rounded-2xl border border-slate-200 dark:border-slate-700 overflow-hidden">
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-slate-50 dark:bg-slate-700/50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Item
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Code
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Quantity
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Status
                        </th>
                        <th className="px-6 py-3 text-right text-xs font-medium text-slate-500 uppercase tracking-wider">
                          Actions
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200 dark:divide-slate-700">
                      {stockItems.length === 0 ? (
                        <tr>
                          <td colSpan={5} className="px-6 py-12 text-center text-slate-500">
                            No stock items found
                          </td>
                        </tr>
                      ) : (
                        stockItems.map(item => (
                          <tr key={item.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/50">
                            <td className="px-6 py-4 whitespace-nowrap">
                              <div className="flex items-center gap-3">
                                <div className="w-8 h-8 rounded-lg bg-purple-100 dark:bg-purple-900/30 flex items-center justify-center">
                                  <Package className="w-4 h-4 text-purple-600" />
                                </div>
                                <span className="font-medium text-slate-800 dark:text-white">
                                  {item.itemName}
                                </span>
                              </div>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-slate-500">
                              {item.itemCode}
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                              <span className={`font-semibold ${
                                item.quantity < 10 ? "text-amber-600" : "text-slate-800 dark:text-white"
                              }`}>
                                {item.quantity}
                              </span>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap">
                              <span
                                className={`px-2 py-1 rounded-full text-xs font-medium ${
                                  item.quantity < 10
                                    ? "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400"
                                    : "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400"
                                }`}
                              >
                                {item.quantity < 10 ? "Low Stock" : "In Stock"}
                              </span>
                            </td>
                            <td className="px-6 py-4 whitespace-nowrap text-right">
                              <button className="text-blue-600 hover:text-blue-700 font-medium text-sm">
                                Edit
                              </button>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
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
