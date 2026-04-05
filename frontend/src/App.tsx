import { Navigate, Route, Routes } from 'react-router-dom'
import './App.css'
import { AppLayout } from './components/layouts/AppLayout'
import { PlanningLayout } from './components/layouts/PlanningLayout'
import { SupervisorLayout } from './components/layouts/SupervisorLayout'
import { BomImportBatchDetailPage } from './pages/BomImport/BomImportBatchDetailPage'
import { BomImportPage } from './pages/BomImport/BomImportPage'
import { ImportHistoryPage } from './pages/BomImport/ImportHistoryPage'
import { MaterialRequirementsPage } from './pages/MaterialRequirements/MaterialRequirementsPage'
import { PlanningHomePage } from './pages/PlanningHome/PlanningHomePage'
import { RoutingSetupPage } from './pages/RoutingSetup/RoutingSetupPage'
import { SupervisorHomePage } from './pages/SupervisorHome/SupervisorHomePage'
import { WorkOrderPrepPage } from './pages/WorkOrderPrep/WorkOrderPrepPage'
import { WorkOrderDetailPage } from './pages/WorkOrderDetail/WorkOrderDetailPage'
import { WorkCenterQueuePage } from './pages/WorkCenterQueue/WorkCenterQueuePage'
import { WorkOrdersBoardPage } from './pages/WorkOrdersBoard/WorkOrdersBoardPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<AppLayout />}>
        <Route index element={<Navigate to="/supervisor/work-orders" replace />} />

        <Route path="supervisor" element={<SupervisorLayout />}>
          <Route index element={<SupervisorHomePage />} />
          <Route path="work-orders" element={<WorkOrdersBoardPage />} />
          <Route path="work-orders/:id" element={<WorkOrderDetailPage />} />
          <Route path="work-centers" element={<WorkCenterQueuePage />} />
        </Route>

        <Route path="planning" element={<PlanningLayout />}>
          <Route index element={<PlanningHomePage />} />
          <Route path="imports" element={<BomImportPage />} />
          <Route path="imports/history" element={<ImportHistoryPage />} />
          <Route path="imports/:id" element={<BomImportBatchDetailPage />} />
          <Route path="material-requirements" element={<MaterialRequirementsPage />} />
          <Route path="routing-setup" element={<RoutingSetupPage />} />
          <Route path="work-order-prep" element={<WorkOrderPrepPage />} />
        </Route>

        <Route path="*" element={<Navigate to="/supervisor/work-orders" replace />} />
      </Route>
    </Routes>
  )
}

export default App
