public class DoubleBuffer<T> {

    private T[] buffer1;
    private T[] buffer2;
    bool currentFirst;

    public DoubleBuffer(int size) {
        buffer1 = new T[size];
        buffer2 = new T[size];
        currentFirst = true;
    }

    public void switchBuffers() {
        currentFirst = !currentFirst;
    }

    public T[] getCurrentBuffer() {
        return currentFirst ? buffer1 : buffer2;
    }

    public T[] getPreviousBuffer() {
        return currentFirst ? buffer2 : buffer1;
    }
}
