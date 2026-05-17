import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SizzlingDashboard } from './components/SizzlingDashboard';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <SizzlingDashboard />
    </QueryClientProvider>
  );
}

export default App;
